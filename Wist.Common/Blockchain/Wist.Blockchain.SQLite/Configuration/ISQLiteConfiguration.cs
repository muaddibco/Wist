using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Configuration;

namespace Wist.Blockchain.SQLite.Configuration
{
    public interface ISQLiteConfiguration : IConfigurationSection
    {

        string ConnectionString { get; set; }

        bool WipeOnStart { get; set; }
    }
}
