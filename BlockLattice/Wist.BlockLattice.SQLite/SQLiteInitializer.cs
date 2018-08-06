using Wist.BlockLattice.SQLite.Configuration;
using Wist.Core;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Configuration;

namespace Wist.BlockLattice.SQLite
{
    [RegisterExtension(typeof(IInitializer), Lifetime = LifetimeManagement.Singleton)]
    public class SQLiteInitializer : InitializerBase
    {
        private readonly SQLiteConfiguration _configuration;

        public SQLiteInitializer(IConfigurationService configurationService)
        {
            _configuration = configurationService.Get<SQLiteConfiguration>();
        }

        protected override void InitializeInner()
        {
            if(_configuration.WipeOnStart)
            {
                LatticeDataService.Instance.WipeAll();
            }

            LatticeDataService.Instance.LoadAllIdentities();
            LatticeDataService.Instance.LoadAllKnownNodeIPs();
        }
    }
}
