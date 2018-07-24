﻿using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Aspects;
using Wist.Core.Configuration;

namespace Wist.BlockLattice.SQLite.Configuration
{
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class SQLiteConfiguration : ConfigurationSectionBase
    {
        public const string SECTION_NAME = "sqlite";

        public SQLiteConfiguration(IApplicationContext applicationContext) : base(applicationContext, SECTION_NAME)
        {
        }

        public string ConnectionString { get; set; }
    }
}
