using Wist.Core;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.PerformanceCounters;
using Wist.Simulation.Load.PerformanceCounters;

namespace Wist.Simulation.Load
{
    [RegisterExtension(typeof(IInitializer), Lifetime = LifetimeManagement.Singleton)]
    public class SimulationLoadInitializer : InitializerBase
    {
        private readonly LoadCountersService _loadCountersService;

        public SimulationLoadInitializer(IPerformanceCountersRepository performanceCountersRepository)
        {
            _loadCountersService = performanceCountersRepository.GetInstance<LoadCountersService>();
        }

        public override ExtensionOrderPriorities Priority => ExtensionOrderPriorities.Normal;

        protected override void InitializeInner()
        {
            _loadCountersService.Setup();
        }
    }
}
