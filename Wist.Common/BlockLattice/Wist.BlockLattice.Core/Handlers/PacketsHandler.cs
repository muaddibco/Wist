﻿using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.ExtensionMethods;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Logging;
using Wist.Core.HashCalculations;
using Wist.Core.PerformanceCounters;
using Wist.BlockLattice.Core.PerformanceCounters;
using Wist.BlockLattice.Core.Parsers;

namespace Wist.BlockLattice.Core.Handlers
{
    [RegisterDefaultImplementation(typeof(IPacketsHandler), Lifetime = LifetimeManagement.Singleton)]
    public class PacketsHandler : IPacketsHandler
    {
        private readonly ILogger _log;
        private readonly BlockingCollection<byte[]> _messagePackets;
        private readonly EndToEndCountersService _endToEndCountersService;
        private readonly PacketHandlingFlow[] _handlingFlows;
        private readonly int _maxDegreeOfParallelism;

        private CancellationToken _cancellationToken;

        public bool IsInitialized { get; private set; }

        public PacketsHandler(IPacketVerifiersRepository packetTypeHandlersFactory, IBlockParsersRepositoriesRepository blockParsersFactoriesRepository, IBlocksHandlersRegistry blocksProcessorFactory, ICoreVerifiersBulkFactory coreVerifiersBulkFactory, IPerformanceCountersRepository performanceCountersRepository, ILoggerService loggerService)
        {
            _log = loggerService.GetLogger(GetType().Name);
            _messagePackets = new BlockingCollection<byte[]>();
            _endToEndCountersService = performanceCountersRepository.GetInstance<EndToEndCountersService>();

            _maxDegreeOfParallelism = 4;

            _handlingFlows = new PacketHandlingFlow[_maxDegreeOfParallelism];

            for (int i = 0; i < _maxDegreeOfParallelism; i++)
            {
                _handlingFlows[i] = new PacketHandlingFlow(i, coreVerifiersBulkFactory, packetTypeHandlersFactory, blockParsersFactoriesRepository, blocksProcessorFactory, performanceCountersRepository, loggerService);
            }
        }

        public void Initialize(CancellationToken ct)
        {
            if (!IsInitialized)
            {
                _cancellationToken = ct;
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
        }

        public void Start()
        {
            _log.Info("PacketsHandler starting");

            Parallel.For(0, _maxDegreeOfParallelism, i => Parse(i));

            _log.Info("PacketsHandler started");
        }

        private async Task Parse(int iteration)
        {
            _log.Info($"Parse function #{iteration} starting");

            try
            {
                _endToEndCountersService.ParallelParsers.Increment();

                foreach (byte[] messagePacket in _messagePackets.GetConsumingEnumerable(_cancellationToken))
                {
                    _endToEndCountersService.MessagesQueueSize.Decrement();
                    await _handlingFlows[iteration].PostMessage(messagePacket);
                }
            }
            finally
            {
                _log.Info("Parse function finished");
                _endToEndCountersService.ParallelParsers.Decrement();
            }
        }
    }
}
