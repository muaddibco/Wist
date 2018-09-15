using CommonServiceLocator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wist.Core.Architecture;
using Wist.Core.PerformanceCounters;
using Wist.Node.Core;

namespace Wist.Setup
{
    public class SetupBootstrapper : NodeBootstrapper
    {
        public SetupBootstrapper(CancellationToken ct) : base(ct)
        {
        }

        protected override void StartNode()
        {
            ServiceLocator.Current.GetInstance<PerformanceCountersInitializer>().Setup();
        }

        protected override void RunInitializers()
        {
        }
    }
}
