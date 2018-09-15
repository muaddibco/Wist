using CommonServiceLocator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wist.Core.Architecture;

namespace Wist.Setup.Simulation
{
    public class SetupBootstrapper : Bootstrapper
    {
        public SetupBootstrapper(CancellationToken ct) : base(ct)
        {
        }

        public bool ResetDatabase { get; set; }

        protected override IEnumerable<string> EnumerateCatalogItems(string rootFolder)
        {
            return base.EnumerateCatalogItems(rootFolder)
                .Concat(Directory.EnumerateFiles(rootFolder, "Wist.BlockLattice.*.dll").Select(f => new FileInfo(f).Name));
        }

        public override void Run()
        {
            base.Run();

            SetupSimulation setupSimulation = ServiceLocator.Current.GetInstance<SetupSimulation>();
            setupSimulation.Run(ResetDatabase);
        }
    }
}
