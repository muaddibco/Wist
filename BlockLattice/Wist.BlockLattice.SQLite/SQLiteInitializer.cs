using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.BlockLattice.SQLite
{
    [RegisterExtension(typeof(IInitializer), Lifetime = LifetimeManagement.Singleton)]
    public class SQLiteInitializer : InitializerBase
    {
        protected override void InitializeInner()
        {
            LatticeDataService.Instance.LoadAllIdentities();
            LatticeDataService.Instance.LoadAllKnownNodeIPs();
        }
    }
}
