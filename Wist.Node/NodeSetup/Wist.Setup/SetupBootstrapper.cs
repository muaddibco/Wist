using CommonServiceLocator;
using System.Threading;
using Wist.Core.Architecture;
using Wist.Core.PerformanceCounters;

namespace Wist.Setup
{
    public class SetupBootstrapper : Bootstrapper
    {
        public SetupBootstrapper(CancellationToken ct) : base(ct)
        {
        }

        protected override void RunInitializers()
        {
            ServiceLocator.Current.GetInstance<PerformanceCountersInitializer>().Setup();
        }
    }
}