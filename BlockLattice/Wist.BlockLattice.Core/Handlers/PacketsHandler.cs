using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Wist.Core;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.ExtensionMethods;
using Wist.BlockLattice.Core.Interfaces;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.DataModel;
using Wist.Core.Logging;
using System.Linq;
using Wist.Core.ProofOfWork;
using Wist.Core.PerformanceCounters;
using Wist.BlockLattice.Core.PerformanceCounters;
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;

namespace Wist.BlockLattice.Core.Handlers
{
    [RegisterDefaultImplementation(typeof(IPacketsHandler), Lifetime = LifetimeManagement.Singleton)]
    public class PacketsHandler : IPacketsHandler
    {
        private readonly ILogger _log;
        private readonly IPacketVerifiersRepository _chainTypeValidationHandlersFactory;
        private readonly IBlockParsersRepositoriesRepository _blockParsersFactoriesRepository;
        private readonly IBlocksHandlersRegistry _blocksProcessorFactory;
        private readonly ICoreVerifiersBulkFactory _coreVerifiersBulkFactory;
        private readonly IProofOfWorkCalculationRepository _proofOfWorkCalculationRepository;
        private readonly BlockingCollection<byte[]> _messagePackets;
        private readonly ManualResetEventSlim _messageTrigger;
        private readonly EndToEndCountersService _endToEndCountersService;
        private readonly PacketHandlingFlow[] _handlingFlows;

        private CancellationTokenSource _cancellationTokenSource;

        public bool IsInitialized { get; private set; }

        public PacketsHandler(IPacketVerifiersRepository packetTypeHandlersFactory, IBlockParsersRepositoriesRepository blockParsersFactoriesRepository, IBlocksHandlersRegistry blocksProcessorFactory, ICoreVerifiersBulkFactory coreVerifiersBulkFactory, IPerformanceCountersRepository performanceCountersRepository, ILoggerService loggerService)
        {
            _log = loggerService.GetLogger(GetType().Name);
            _chainTypeValidationHandlersFactory = packetTypeHandlersFactory;
            _blockParsersFactoriesRepository = blockParsersFactoriesRepository;
            _blocksProcessorFactory = blocksProcessorFactory;
            _coreVerifiersBulkFactory = coreVerifiersBulkFactory;
            _messagePackets = new BlockingCollection<byte[]>();
            _messageTrigger = new ManualResetEventSlim();
            _endToEndCountersService = performanceCountersRepository.GetInstance<EndToEndCountersService>();

            _handlingFlows = new PacketHandlingFlow[Environment.ProcessorCount];


        }

        public void Initialize()
        {
            if(!IsInitialized)
            {
                _handlingFlows = new Thread[Environment.ProcessorCount];
                //for (int i = 0; i < Environment.ProcessorCount; i++)
                //{
                //    _handlingThreads[i] = new Thread(new ThreadStart(Parse)) { IsBackground = true };
                //}
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
            _messagePackets.Add(messagePacket);
            _endToEndCountersService.MessagesQueueSize.Increment();

            _messageTrigger.Set();
        }

        public void Start()
        {
            _log.Info("PacketsHandler starting");

            Stop();

            _cancellationTokenSource = new CancellationTokenSource();
            Parallel.For(1, Environment.ProcessorCount, 
                new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount, CancellationToken = _cancellationTokenSource.Token }, 
                i => Task.Factory.StartNew(() =>  Parse(i), _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default));

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

        private void Parse(int iteration)
        {
            CancellationToken token = _cancellationTokenSource.Token;

            _log.Info($"Parse function #{iteration} starting");

            try
            {
                _endToEndCountersService.ParallelParsers.Increment();

                foreach (byte[] messagePacket in _messagePackets.GetConsumingEnumerable(_cancellationTokenSource.Token))
                {
                    _endToEndCountersService.MessagesQueueSize.Decrement();
                    _handlingFlows[iteration].PostMessage(messagePacket);
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

                    if (blockBase != null)
                    {
                        blockBase.RawData = decodedPacket;

                        DispatchBlock(blockBase);
                    }
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

        private bool ValidatePacket(PacketType packetType, byte[] messagePacket)
        {
            IPacketVerifier packetVerifier = _chainTypeValidationHandlersFactory.GetInstance(packetType);

            bool res = packetVerifier?.ValidatePacket(messagePacket) ?? true;

            return res;
        }

    }
}
