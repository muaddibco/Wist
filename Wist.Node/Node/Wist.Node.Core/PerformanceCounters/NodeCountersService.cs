using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Logging;
using Wist.Core.PerformanceCounters;

namespace Wist.Node.Core.PerformanceCounters
{
    [PerfCounterCategory("Wist Node Measurements")]
    [RegisterExtension(typeof(IPerformanceCountersCategoryBase), Lifetime = LifetimeManagement.Singleton)]
    public class NodeCountersService : PerformanceCountersCategoryBase
    {
        public NodeCountersService(IApplicationContext applicationContext, ILoggerService loggerService) : base(applicationContext, loggerService)
        {
        }

        public override string Name => "NodeCounters";

        [PerfCounter(nameof(MemPoolSize), "Number of packets in MemPool")]
        public IntegerCounter MemPoolSize { get; set; }


        [PerfCounter(nameof(RegistryBlockLastSize), "Number of transaction headers included into last Registry Block")]
        public IntegerCounter RegistryBlockLastSize { get; set; }
    }
}
