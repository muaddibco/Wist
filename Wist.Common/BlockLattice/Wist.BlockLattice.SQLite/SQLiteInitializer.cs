using Wist.BlockLattice.SQLite.Configuration;
using Wist.BlockLattice.SQLite.DataAccess;
using Wist.Core;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Configuration;

namespace Wist.BlockLattice.SQLite
{
    [RegisterExtension(typeof(IInitializer), Lifetime = LifetimeManagement.Singleton)]
    public class SQLiteInitializer : InitializerBase
    {
        private readonly ISQLiteConfiguration _configuration;

        public SQLiteInitializer(IConfigurationService configurationService)
        {
            _configuration = configurationService.Get<ISQLiteConfiguration>();
        }

        public override ExtensionOrderPriorities Priority => ExtensionOrderPriorities.Highest;

        protected override void InitializeInner()
        {
            if(_configuration.WipeOnStart)
            {
                DataAccessService.Instance.WipeAll();
            }

            DataAccessService.Instance.LoadAllIdentities();
            DataAccessService.Instance.LoadAllKnownNodeIPs();
        }
    }
}
