using System;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Logging;

namespace Wist.Core.PerformanceCounters
{
    [RegisterExtension(typeof(IInitializer), Lifetime = LifetimeManagement.Singleton)]
    public class PerformanceCountersInitializer : InitializerBase
    {
        private readonly IPerformanceCountersCategoryBase[] _performanceCountersCategoryBases;
        private readonly ILogger _log;

        public override ExtensionOrderPriorities Priority => ExtensionOrderPriorities.Normal;

        public PerformanceCountersInitializer(IPerformanceCountersCategoryBase[] performanceCountersCategoryBases, ILoggerService loggerService)
        {
            _performanceCountersCategoryBases = performanceCountersCategoryBases;
            _log = loggerService.GetLogger(GetType().Name);
        }

        protected override void InitializeInner()
        {
            foreach (IPerformanceCountersCategoryBase perfCounterCategory in _performanceCountersCategoryBases)
            {
                try
                {
                    perfCounterCategory.Initialize();
                }
                catch (Exception ex)
                {
                    _log.Error($"Failed to initialize PerformanceCategory {perfCounterCategory.Name}", ex);
                }
            }
        }

        public void Setup()
        {
            foreach (IPerformanceCountersCategoryBase perfCounterCategory in _performanceCountersCategoryBases)
            {
                try
                {
                    perfCounterCategory.Setup();
                }
                catch (Exception ex)
                {
                    _log.Error($"Failed to initialize PerformanceCategory {perfCounterCategory.Name}", ex);
                }
            }
        }
    }
}
