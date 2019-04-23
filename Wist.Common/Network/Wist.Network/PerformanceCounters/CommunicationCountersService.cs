using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Logging;
using Wist.Core.PerformanceCounters;

namespace Wist.Network.PerformanceCounters
{
    [PerfCounterCategory("Wist Communication Measurements")]
    [RegisterExtension(typeof(IPerformanceCountersCategoryBase), Lifetime = LifetimeManagement.Singleton)]
    public class CommunicationCountersService : PerformanceCountersCategoryBase
    {
        public CommunicationCountersService(IApplicationContext applicationContext, ILoggerService loggerService) : base(applicationContext, loggerService)
        {
        }

        public override string Name => "CommunicationCounters";

        [PerfCounter(nameof(CommunicationChannels), "Number of active communication channels")]
        public IntegerCounter CommunicationChannels { get; set; }

        [PerfCounter(nameof(BytesReceived), "Throughput of received bytes")]
        public RateOfCountsPerSecondCounter BytesReceived { get; set; }

        [PerfCounter(nameof(BytesSent), "Throughput of sent bytes")]
        public RateOfCountsPerSecondCounter BytesSent { get; set; }

        [PerfCounter(nameof(CommunicationErrors), "Rate of socket errors")]
        public RateOfCountsPerSecondCounter CommunicationErrors { get; set; }

        [PerfCounter(nameof(ParsingQueueSize), "Size of parsing queue")]
        public IntegerCounter ParsingQueueSize { get; set; }
    }
}
