using CommonServiceLocator;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Wist.Core.Architecture;

namespace Wist.Node.Core
{
    public class NodeBootstrapper : Bootstrapper
    {
        private readonly string[] _catalogItems = new string[] { "Wist.Crypto.dll", "Chaos.NaCl.dll", "Wist.Communication.dll", "Wist.Node.Core.dll" };

        public NodeBootstrapper(CancellationToken ct) : base(ct)
        {
        }

        public override void Run()
        {
            _log.Info("Starting NodeBootstrap Run");
            try
            {
                base.Run();

                StartNode();
            }
            finally
            {
                _log.Info("NodeBootstrap Run completed");
            }
        }

        protected override IEnumerable<string> EnumerateCatalogItems(string rootFolder)
        {
            return base.EnumerateCatalogItems(rootFolder)
                .Concat(_catalogItems)
                .Concat(Directory.EnumerateFiles(rootFolder, "Wist.BlockLattice.*.dll").Select(f => new FileInfo(f).Name));
        }

        #region Private Functions

        protected virtual void StartNode()
        {
            _log.Info("Starting Node");
            try
            {
                NodeMain nodeMain = ServiceLocator.Current.GetInstance<NodeMain>();

                nodeMain.Initialize(_cancellationToken);

                nodeMain.Start();
            }
            finally
            {
                _log.Info("Starting Node completed");
            }
        }

        #endregion Private Functions
    }
}
