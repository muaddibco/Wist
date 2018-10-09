using CommonServiceLocator;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Wist.Core.Architecture;
using Wist.Core.PerformanceCounters;

namespace Wist.Setup
{
    public class SetupBootstrapper : Bootstrapper
    {
        private readonly string[] _catalogItems = new string[] { "Wist.Crypto.dll", "Chaos.NaCl.dll", "Wist.Network.dll", "Wist.Node.Core.dll" };

        public SetupBootstrapper(CancellationToken ct) : base(ct)
        {
        }


        protected override IEnumerable<string> EnumerateCatalogItems(string rootFolder)
        {
            return base.EnumerateCatalogItems(rootFolder)
                .Concat(_catalogItems)
                .Concat(Directory.EnumerateFiles(rootFolder, "Wist.BlockLattice.*.dll").Select(f => new FileInfo(f).Name));
        }

        protected override void RunInitializers()
        {
            ServiceLocator.Current.GetInstance<PerformanceCountersInitializer>().Setup();
        }
    }
}