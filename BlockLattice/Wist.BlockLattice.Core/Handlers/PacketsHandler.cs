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
using Wist.Core.Logging;

namespace Wist.BlockLattice.Core.Handlers
{
    [RegisterDefaultImplementation(typeof(IPacketsHandler), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class PacketsHandler : IPacketsHandler
    {
        private readonly ILogger _log;
        private readonly IPacketVerifiersRepository _chainTypeValidationHandlersFactory;
        private readonly IBlockParsersFactoriesRepository _blockParsersFactoriesRepository;
        private readonly IBlocksHandlersFactory _blocksProcessorFactory;
        private readonly ConcurrentQueue<byte[]> _messagePackets;
        private readonly ManualResetEventSlim _messageTrigger;

        private CancellationTokenSource _cancellationTokenSource;

        public bool IsInitialized { get; private set; }

        public PacketsHandler(IPacketVerifiersRepository packetTypeHandlersFactory, IBlockParsersFactoriesRepository blockParsersFactoriesRepository, IBlocksHandlersFactory blocksProcessorFactory, ILoggerService loggerService)
        {
            _log = loggerService.GetLogger(GetType().Name);
            _chainTypeValidationHandlersFactory = packetTypeHandlersFactory;
            _blockParsersFactoriesRepository = blockParsersFactoriesRepository;
            _blocksProcessorFactory = blocksProcessorFactory;
            _messagePackets = new ConcurrentQueue<byte[]>();
            _messageTrigger = new ManualResetEventSlim();
        }

        public void Initialize()
        {
            if(!IsInitialized)
            {
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

        public void Start()
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
                _log.Warning("An EMPTY packet obtained at ProcessMessagePacket");
                return;
            }

            IPacketVerifier chainTypeHandler = null;

            PacketType packetType = (PacketType)BitConverter.ToUInt16(messagePacket, 0);
            chainTypeHandler = _chainTypeValidationHandlersFactory.GetInstance(packetType);

            try
            {
                bool res = chainTypeHandler.ValidatePacket(messagePacket);

                if(res)
                {
                    IBlockParsersFactory blockParsersFactory = _blockParsersFactoriesRepository.GetBlockParsersFactory(packetType);
                    BlockBase blockBase = ParseMessagePacket(blockParsersFactory, messagePacket);

                    DispatchBlock(blockBase);
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to process packet {messagePacket.ToHexString()}", ex);
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
                IEnumerable<IBlocksHandler> blocksProcessors = _blocksProcessorFactory.GetBulkInstances(block.PacketType);

                //TODO: weigh make it in parallel
                foreach (IBlocksHandler blocksProcessor in blocksProcessors)
                {
                    blocksProcessor.ProcessBlock(block);
                }
            }
        }
    }
}
