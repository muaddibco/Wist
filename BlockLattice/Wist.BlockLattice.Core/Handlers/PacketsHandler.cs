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
using Wist.Core.Aspects;

namespace Wist.BlockLattice.Core.Handlers
{
    [InitializationMandatory]
    [RegisterDefaultImplementation(typeof(IPacketsHandler), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class PacketsHandler : IPacketsHandler, ISupportInitialization
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(PacketsHandler));
        private readonly IChainTypeHandlersFactory _chainTypeValidationHandlersFactory;
        private readonly IBlockParsersFactory _blockParsersFactory;
        private readonly ConcurrentQueue<byte[]> _messagePackets;
        private readonly ManualResetEventSlim _messageTrigger;
        private readonly ConcurrentQueue<PacketErrorMessage> _messageErrorPackets;
        private readonly ManualResetEventSlim _messageErrorTrigger;

        private IBlocksProcessor _blocksProcessor;
        private CancellationTokenSource _cancellationTokenSource;

        public bool IsInitialized { get; private set; }

        public PacketsHandler(IChainTypeHandlersFactory packetTypeHandlersFactory, IBlockParsersFactory blockParsersFactory)
        {
            _chainTypeValidationHandlersFactory = packetTypeHandlersFactory;
            _blockParsersFactory = blockParsersFactory;
            _messagePackets = new ConcurrentQueue<byte[]>();
            _messageTrigger = new ManualResetEventSlim();
            _messageErrorPackets = new ConcurrentQueue<PacketErrorMessage>();
            _messageErrorTrigger = new ManualResetEventSlim();
        }

        public void Initialize(IBlocksProcessor blocksProcessor)
        {
            if(!IsInitialized)
            {
                _blocksProcessor = blocksProcessor;
                IsInitialized = true;
            }
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

            IChainTypeHandler chainTypeHandler = null;

            ChainType chainType = (ChainType)BitConverter.ToUInt16(messagePacket, 0);
            chainTypeHandler = _chainTypeValidationHandlersFactory.Create(chainType);

            try
            {
                PacketErrorMessage packetErrorMessage = chainTypeHandler.ValidatePacket(messagePacket);
                if (packetErrorMessage.ErrorCode != PacketsErrors.NO_ERROR)
                {
                    PushPacketErrorPacket(packetErrorMessage);
                }
                else
                {
                    BlockBase blockBase = ParseMessagePacket(chainTypeHandler.BlockParsersFactory, messagePacket);

                    DispatchBlock(blockBase);
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to process packet {messagePacket.ToHexString()}", ex);
            }
            finally
            {
                if (chainTypeHandler != null)
                {
                    _chainTypeValidationHandlersFactory.Utilize(chainTypeHandler);
                }
            }
        }

        private BlockBase ParseMessagePacket(IBlockParsersFactory blockParsersFactory, byte[] messagePacket)
        {
            BlockBase blockBase = null;
            IBlockParser blockParser = null;

            try
            {
                ushort blockType = BitConverter.ToUInt16(messagePacket, 2);

                blockParser = blockParsersFactory.Create(blockType);

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
                    blockParsersFactory.Utilize(blockParser);
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
