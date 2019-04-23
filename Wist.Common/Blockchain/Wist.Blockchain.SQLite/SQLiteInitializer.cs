using System.Threading;
using Wist.Blockchain.SQLite.Configuration;
using Wist.Blockchain.SQLite.DataAccess;
using Wist.Core;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Configuration;

namespace Wist.Blockchain.SQLite
{
    [RegisterExtension(typeof(IInitializer), Lifetime = LifetimeManagement.Singleton)]
    public class SQLiteInitializer : InitializerBase
    {
        private ISQLiteConfiguration _configuration;
        private readonly IConfigurationService _configurationService;

        public SQLiteInitializer(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }

        public override ExtensionOrderPriorities Priority => ExtensionOrderPriorities.Highest;

        protected override void InitializeInner(CancellationToken cancellationToken)
        {
            _configuration = _configurationService.Get<ISQLiteConfiguration>();
            if (_configuration.WipeOnStart)
            {
                DataAccessService.Instance.WipeAll();
            }

            DataAccessService.Instance.LoadAllIdentities();
            DataAccessService.Instance.LoadAllKnownNodeIPs();
            DataAccessService.Instance.LoadAllImageKeys();
        }
    }
}
