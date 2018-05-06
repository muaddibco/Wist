using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wist.Core;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.ExtensionMethods;
using Wist.Core.Models;
using Wist.BlockLattice.Core.Interfaces;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.DataModel;

namespace Wist.BlockLattice.Core.Handlers
{
    [RegisterDefaultImplementation(typeof(IPacketsHandler), Lifetime = LifetimeManagement.Singleton)]
    public class PacketsHandler : IPacketsHandler
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(PacketsHandler));
        private readonly IChainTypeValidationHandlersFactory _chainTypeValidationHandlersFactory;
        private readonly IBlockParsersFactory _blockParsersFactory;
        private readonly IBlocksProcessor _blocksProcessor;
        private readonly ConcurrentQueue<byte[]> _messagePackets;
        private readonly ManualResetEventSlim _messageTrigger;
        private readonly ConcurrentQueue<PacketErrorMessage> _messageErrorPackets;
        private readonly ManualResetEventSlim _messageErrorTrigger;

        private CancellationTokenSource _cancellationTokenSource;

        public PacketsHandler(IChainTypeValidationHandlersFactory packetTypeHandlersFactory, IBlockParsersFactory blockParsersFactory, IBlocksProcessor blocksProcessor)
        {
            _chainTypeValidationHandlersFactory = packetTypeHandlersFactory;
            _blockParsersFactory = blockParsersFactory;
            _blocksProcessor = blocksProcessor;
            _messagePackets = new ConcurrentQueue<byte[]>();
            _messageTrigger = new ManualResetEventSlim();
            _messageErrorPackets = new ConcurrentQueue<PacketErrorMessage>();
            _messageErrorTrigger = new ManualResetEventSlim();
        }

        /// <summary>
        /// Bytes being pushed to <see cref="IPacketsHandler"/> must form complete packet for following validation and processing
        /// </summary>
        /// <param name="messagePacket">Bytes of complete message for following processing</param>
        public void Push(byte[] messagePacket)
        {
            _messagePackets.Enqueue(messagePacket);
            _messageTrigger.Set();
        }

        public void Start(bool withErrorsProcessing = true)
        {
            Stop();

            _cancellationTokenSource = new CancellationTokenSource();

            Task.Factory.StartNew(() => 
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    _messageTrigger.Reset();
                    Parse(_cancellationTokenSource.Token);
                    _messageTrigger.Wait(_cancellationTokenSource.Token);
                }
            }, TaskCreationOptions.LongRunning);

            PeriodicTaskFactory.Start(() => 
            {
                if(_messagePackets.Count > 100)
                {
                    Task.Factory.StartNew(() => Parse(_cancellationTokenSource.Token), TaskCreationOptions.LongRunning);
                }
            }, 1000, cancelToken: _cancellationTokenSource.Token, delayInMilliseconds: 3000);

            if(withErrorsProcessing)
            {
                Task.Factory.StartNew(() => ProcessPacketErrorPackets(_cancellationTokenSource.Token), TaskCreationOptions.LongRunning);
            }
        }

        public void Stop()
        {
            _messageTrigger?.Set();
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
        }

        private void ParseMain(CancellationToken token)
        {
            while(!token.IsCancellationRequested)
            {
                _messageTrigger.Reset();
                Parse(token);
                _messageTrigger.Wait();
            }
        }

        private void Parse(CancellationToken token)
        {
            byte[] messagePacket;
            while (!token.IsCancellationRequested && _messagePackets.TryDequeue(out messagePacket))
            {
                object @message = messagePacket;
                Task.Factory.StartNew(m => ProcessMessagePacket((byte[])m), @message, token);
            }
        }

        private void ProcessMessagePacket(byte[] messagePacket)
        {
            if (messagePacket == null)
            {
                _log.Warn("An EMPTY packet obtained at ProcessMessagePacket");
                return;
            }

            if (!ValidateByChainType(messagePacket))
                return;

            BlockBase blockBase = ParseMessagePacket(messagePacket);

            DispatchBlock(blockBase);
        }

        private bool ValidateByChainType(byte[] messagePacket)
        {
            IChainTypeValidationHandler chainTypeValidationHandler = null;
            try
            {
                ChainType chainType = (ChainType)BitConverter.ToUInt16(messagePacket, 0);
                chainTypeValidationHandler = _chainTypeValidationHandlersFactory.Create(chainType);

                PacketErrorMessage packetErrorMessage = chainTypeValidationHandler.ProcessPacket(messagePacket);
                if (packetErrorMessage.ErrorCode != PacketsErrors.NO_ERROR)
                {
                    PushPacketErrorPacket(packetErrorMessage);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to process packet {messagePacket.ToHexString()}", ex);
                return false;
            }
            finally
            {
                if (chainTypeValidationHandler != null)
                {
                    _chainTypeValidationHandlersFactory.Utilize(chainTypeValidationHandler);
                }
            }

            return true;
        }

        private BlockBase ParseMessagePacket(byte[] messagePacket)
        {
            BlockBase blockBase = null;
            IBlockParser blockParser = null;

            try
            {
                BlockType blockType = (BlockType)BitConverter.ToUInt16(messagePacket, 2);

                blockParser = _blockParsersFactory.Create(blockType);

                blockBase = blockParser.Parse(messagePacket);
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to parse message {messagePacket.ToHexString()}", ex);
            }
            finally
            {
                if (blockParser != null)
                {
                    _blockParsersFactory.Utilize(blockParser);
                }
            }

            return blockBase;
        }

        private void DispatchBlock(BlockBase block)
        {
            if (block != null)
            {
                _blocksProcessor.ProcessBlock(block);
            }
        }

        #region Packets errors processing

        private void PushPacketErrorPacket(PacketErrorMessage messageErrorPacket)
        {
            _messageErrorPackets.Enqueue(messageErrorPacket);
            _messageErrorTrigger.Set();
        }

        private void ProcessPacketErrorPackets(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                PacketErrorMessage messageErrorPacket;
                if (_messageErrorPackets.TryDequeue(out messageErrorPacket))
                {
                    _messageErrorTrigger.Reset();
                    switch (messageErrorPacket.ErrorCode)
                    {
                        case PacketsErrors.LENGTH_IS_INVALID:
                            _log.Error($"Invalid message length 0: {messageErrorPacket.MessagePacket.ToHexString()}");
                            break;
                        case PacketsErrors.LENGTH_DOES_NOT_MATCH:
                            _log.Error($"Actual message body length is differs from declared: {messageErrorPacket.MessagePacket.ToHexString()}");
                            break;
                        case PacketsErrors.SIGNATURE_IS_INVALID:
                            _log.Error($"Message signature is invalid: {messageErrorPacket.MessagePacket.ToHexString()}");
                            break;
                        case PacketsErrors.HASHBACK_IS_INVALID:
                            _log.Error($"Message hash N back value is invalid: {messageErrorPacket.MessagePacket.ToHexString()}");
                            break;
                    }
                }
                else
                {
                    _messageErrorTrigger.Wait();
                }
            }
        }

        #endregion Packets errors processing
    }
}
