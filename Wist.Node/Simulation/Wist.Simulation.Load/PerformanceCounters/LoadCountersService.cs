using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Logging;
using Wist.Core.PerformanceCounters;

namespace Wist.Simulation.Load.PerformanceCounters
{
    [PerfCounterCategory("Wist Load Measurements")]
    [RegisterExtension(typeof(IPerformanceCountersCategoryBase), Lifetime = LifetimeManagement.Singleton)]
    public class LoadCountersService : PerformanceCountersCategoryBase
    {
        public LoadCountersService(IApplicationContext applicationContext, ILoggerService loggerService) : base(applicationContext, loggerService)
        {
        }

        public override string Name => "LoadCounters";

        [PerfCounter(nameof(SentMessages), "Throughput of sent messages")]
        public RateOfCountsPerSecondCounter SentMessages { get; set; }
    }
}
