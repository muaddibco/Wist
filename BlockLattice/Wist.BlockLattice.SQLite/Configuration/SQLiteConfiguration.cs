using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Aspects;
using Wist.Core.Configuration;

namespace Wist.BlockLattice.SQLite.Configuration
{
    [ConfigurationSectionSupport]
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class SQLiteConfiguration : IConfigurationSection
    {
        public string SectionName => "sqlite";

        public string ConnectionString { get; set; }
    }
}
