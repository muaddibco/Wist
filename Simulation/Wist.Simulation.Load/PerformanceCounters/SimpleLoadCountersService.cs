using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.PerformanceCounters;

namespace Wist.Simulation.Load.PerformanceCounters
{
    [PerfCounterCategory("Simple Load Measurements")]
    [RegisterExtension(typeof(IPerformanceCountersCategoryBase), Lifetime = LifetimeManagement.Singleton)]
    public class SimpleLoadCountersService : PerformanceCountersCategoryBase
    {
        public SimpleLoadCountersService(IApplicationContext applicationContext) : base(applicationContext)
        {
        }

        public override string Name => "SimpleLoadCounters";

        [PerfCounter(nameof(SentMessages), "Throughput of sent messages")]
        public RateOfCountsPerSecondCounter SentMessages { get; set; }
    }
}
