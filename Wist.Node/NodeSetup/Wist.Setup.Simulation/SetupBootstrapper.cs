using CommonServiceLocator;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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
            return base.EnumerateCatalogItems(rootFolder).Concat(new string[] { "Wist.Crypto.dll" })
                .Concat(Directory.EnumerateFiles(rootFolder, "Wist.Blockchain.*.dll").Select(f => new FileInfo(f).Name));
        }

        public override void Run(IDictionary<string, string> args = null)
        {
            base.Run();

            SetupSimulation setupSimulation = ServiceLocator.Current.GetInstance<SetupSimulation>();
            setupSimulation.Run(ResetDatabase);
        }
    }
}
