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
using System.Linq;
using System.Threading.Tasks.Dataflow;
using Wist.Core.ProofOfWork;
using Wist.Core.PerformanceCounters;
using Wist.BlockLattice.Core.PerformanceCounters;
using System.Diagnostics;

namespace Wist.BlockLattice.Core.Handlers
{
    [RegisterDefaultImplementation(typeof(IPacketsHandler), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class PacketsHandler : IPacketsHandler
    {
        private const byte DLE = 0x10;
        private readonly ILogger _log;
        private readonly IPacketVerifiersRepository _chainTypeValidationHandlersFactory;
        private readonly IBlockParsersRepositoriesRepository _blockParsersFactoriesRepository;
        private readonly IBlocksHandlersFactory _blocksProcessorFactory;
        private readonly IProofOfWorkCalculationRepository _proofOfWorkCalculationRepository;
        private readonly ConcurrentQueue<byte[]> _messagePackets;
        private readonly ManualResetEventSlim _messageTrigger;
        private readonly EndToEndCountersService _endToEndCountersService;

        private CancellationTokenSource _cancellationTokenSource;

        public bool IsInitialized { get; private set; }

        public PacketsHandler(IPacketVerifiersRepository packetTypeHandlersFactory, IBlockParsersRepositoriesRepository blockParsersFactoriesRepository, IBlocksHandlersFactory blocksProcessorFactory, IProofOfWorkCalculationRepository proofOfWorkCalculationFactory, IPerformanceCountersRepository performanceCountersRepository, ILoggerService loggerService)
        {
            _log = loggerService.GetLogger(GetType().Name);
            _chainTypeValidationHandlersFactory = packetTypeHandlersFactory;
            _blockParsersFactoriesRepository = blockParsersFactoriesRepository;
            _blocksProcessorFactory = blocksProcessorFactory;
            _proofOfWorkCalculationRepository = proofOfWorkCalculationFactory;
            _messagePackets = new ConcurrentQueue<byte[]>();
            _messageTrigger = new ManualResetEventSlim();
            _endToEndCountersService = performanceCountersRepository.GetInstance<EndToEndCountersService>();
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
            _endToEndCountersService.PushedForHandlingTransactionsThroughput.Increment();
            _log.Debug($"Pushed packer for handling: {messagePacket.ToHexString()}");
            _messagePackets.Enqueue(messagePacket);
            _endToEndCountersService.MessagesQueueSize.Increment();

            _messageTrigger.Set();
        }

        public void Start()
        {
            _log.Info("PacketsHandler starting");

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
                if (_endToEndCountersService.ParallelParsers.RawValue > 0 && _messagePackets.Count / 10000 > _endToEndCountersService.ParallelParsers.RawValue)
                {
                    Task.Factory.StartNew(() => Parse(_cancellationTokenSource.Token), TaskCreationOptions.LongRunning);
                }
            }, 3000, cancelToken: _cancellationTokenSource.Token, delayInMilliseconds: 3000);

            _log.Info("PacketsHandler started");
        }

        public void Stop()
        {
            _log.Info("PacketsHandler stopping");
            _messageTrigger?.Set();
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
            _log.Info("PacketsHandler stopped");
        }

        private void Parse(CancellationToken token)
        {
            _log.Info("Parse function starting");

            try
            {
                _endToEndCountersService.ParallelParsers.Increment();

                byte[] messagePacket;
                while (!token.IsCancellationRequested && _messagePackets.TryDequeue(out messagePacket))
                {
                    _endToEndCountersService.MessagesQueueSize.Decrement();
                    ProcessMessagePacket(messagePacket);
                }
            }
            finally
            {
                _log.Info("Parse function finished");
                _endToEndCountersService.ParallelParsers.Decrement();
            }
        }

        private void ProcessMessagePacket(byte[] messagePacket)
        {
            var handlingStopwatch = Stopwatch.StartNew();
            _endToEndCountersService.HandlingTransactionsThroughput.Increment();

            _log.Debug("ProcessMessagePacket started");

            if (messagePacket == null)
            {
                _log.Warning("An EMPTY packet obtained at ProcessMessagePacket");
                return;
            }

            try
            {
                byte[] decodedPacket = DecodeMessage(messagePacket);

                PacketType packetType = (PacketType)BitConverter.ToUInt16(decodedPacket, 0);

                bool res = ValidatePacket(packetType, decodedPacket);

                if (res)
                {
                    BlockBase blockBase = ParseMessagePacket(packetType, decodedPacket);

                    blockBase.RawData = messagePacket;

                    DispatchBlock(blockBase);
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to process packet {messagePacket.ToHexString()}", ex);
            }
            finally
            {
                handlingStopwatch.Stop();
                _endToEndCountersService.PacketHandlingTimeMeasure.IncrementBy(handlingStopwatch.Elapsed);
            }
        }

        private byte[] DecodeMessage(byte[] messagePacket)
        {
            MemoryStream memoryStream = new MemoryStream();

            bool dleDetected = false;

            foreach (byte b in messagePacket)
            {
                if(b != DLE)
                {
                    if (dleDetected)
                    {
                        dleDetected = false;
                        memoryStream.WriteByte((byte)(b - DLE));
                    }
                    else
                    {
                        memoryStream.WriteByte(b);
                    }
                }
                else
                {
                    dleDetected = true;
                }
            }

            return memoryStream.ToArray();
        }

        private bool ValidatePacket(PacketType packetType, byte[] messagePacket)
        {
            IPacketVerifier packetVerifier = _chainTypeValidationHandlersFactory.GetInstance(packetType);

            bool res = packetVerifier?.ValidatePacket(messagePacket) ?? true;

            return res;
        }

        private BlockBase ParseMessagePacket(PacketType packetType, byte[] messagePacket)
        {
            BlockBase blockBase = null;
            IBlockParser blockParser = null;
            IBlockParsersRepository blockParsersFactory = null;

            try
            {
                //TODO: weigh assumption that all messages are sync based (have reference to latest Sync Block)

                POWType powType = (POWType)BitConverter.ToUInt16(messagePacket, 10);

                IProofOfWorkCalculation proofOfWorkCalculation = _proofOfWorkCalculationRepository.GetInstance(powType);

                int powSize = 0;

                if(powType != POWType.None)
                {
                    powSize = 8 + proofOfWorkCalculation.HashSize;
                }

                ushort blockType = BitConverter.ToUInt16(messagePacket, 12 + powSize + 2);

                blockParsersFactory = _blockParsersFactoriesRepository.GetBlockParsersRepository(packetType);

                blockParser = blockParsersFactory.GetInstance(blockType);

                blockBase = blockParser.Parse(messagePacket);
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to parse message {messagePacket.ToHexString()}", ex);
            }

            return blockBase;
        }

        private void DispatchBlock(BlockBase block)
        {
            if (block != null)
            {
                IEnumerable<IBlocksHandler> blocksProcessors = _blocksProcessorFactory.GetBulkInstances(block.PacketType);

                //TODO: weigh to check whether number of processors is greater than 1 before parallelizing for sake of performance
                blocksProcessors.AsParallel().ForAll(p => p.ProcessBlock(block));
            }
        }
    }
}
