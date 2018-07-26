using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.PerformanceCounters;

namespace Wist.BlockLattice.Core.PerformanceCounters
{
    [PerfCounterCategory("Wist E2E Measurements")]
    [RegisterExtension(typeof(IPerformanceCountersCategoryBase), Lifetime = LifetimeManagement.Singleton)]
    public class EndToEndCountersService : PerformanceCountersCategoryBase
    {
        public EndToEndCountersService(IApplicationContext applicationContext) : base(applicationContext)
        {
        }

        public override string Name => "EndToEndCounters";

        [PerfCounter(nameof(HandlingTransactionsThroughput), "Throughput of transactions being handled")]
        public RateOfCountsPerSecondCounter HandlingTransactionsThroughput { get; set; }

        [PerfCounter(nameof(PushedForHandlingTransactionsThroughput), "Throughput of transactions pushed for handling")]
        public RateOfCountsPerSecondCounter PushedForHandlingTransactionsThroughput { get; set; }
    }
}
