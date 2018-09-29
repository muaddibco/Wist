using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Logging;
using Wist.Core.PerformanceCounters;

namespace Wist.BlockLattice.Core.PerformanceCounters
{
    [PerfCounterCategory("Wist E2E Measurements")]
    [RegisterExtension(typeof(IPerformanceCountersCategoryBase), Lifetime = LifetimeManagement.Singleton)]
    public class EndToEndCountersService : PerformanceCountersCategoryBase
    {
        public EndToEndCountersService(IApplicationContext applicationContext, ILoggerService loggerService) : base(applicationContext, loggerService)
        {
        }

        public override string Name => "EndToEndCounters";

        [PerfCounter(nameof(DecodingThroughput), "Throughput of decodings per second")]
        public RateOfCountsPerSecondCounter DecodingThroughput { get; set; }

        [PerfCounter(nameof(ParsingThroughput), "Throughput of parsings per second")]
        public RateOfCountsPerSecondCounter ParsingThroughput { get; set; }

        [PerfCounter(nameof(CoreValidationThroughput), "Throughput of core validations per second")]
        public RateOfCountsPerSecondCounter CoreValidationThroughput { get; set; }

        [PerfCounter(nameof(DispatchThroughput), "Throughput of dispatches per second")]
        public RateOfCountsPerSecondCounter DispatchThroughput { get; set; }

        [PerfCounter(nameof(ParallelParsers), "Number of parsers working in parallel")]
        public IntegerCounter ParallelParsers { get; set; }

        [PerfCounter(nameof(MessagesQueueSize), "Size of messages queue of Packets Handler")]
        public IntegerCounter MessagesQueueSize { get; set; }

        [PerfCounter(nameof(PushedForHandlingTransactionsThroughput), "Throughput of transactions pushed for handling")]
        public RateOfCountsPerSecondCounter PushedForHandlingTransactionsThroughput { get; set; }

        [PerfCounter(nameof(PacketHandlingTimeMeasure), "Measurement of time spent per message handling")]
        public TimespanCounter PacketHandlingTimeMeasure { get; set; }
    }
}
