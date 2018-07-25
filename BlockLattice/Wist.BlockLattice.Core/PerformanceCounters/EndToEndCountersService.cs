using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.PerformanceCounters;

namespace Wist.BlockLattice.Core.PerformanceCounters
{
    [PerfCounterCategory("WistTransactionsMeasurement")]
    [RegisterExtension(typeof(IPerformanceCountersCategoryBase), Lifetime = LifetimeManagement.Singleton)]
    public class EndToEndCountersService : PerformanceCountersCategoryBase
    {
        public override string Name => "EndToEndCounters";

        [PerfCounter(nameof(TransactionsThroughput), "End to end transactions throughput")]
        public RateOfCountsPerSecondCounter TransactionsThroughput { get; set; }
    }
}
