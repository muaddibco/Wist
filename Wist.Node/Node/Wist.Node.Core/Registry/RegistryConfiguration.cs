using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Aspects;
using Wist.Core.Configuration;

namespace Wist.Node.Core.Registry
{
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class RegistryConfiguration : ConfigurationSectionBase, IRegistryConfiguration
    {
        public const string SECTION_NAME = "registry";

        public RegistryConfiguration(IApplicationContext applicationContext) : base(applicationContext, SECTION_NAME)
        {
        }

        public string TcpServiceName { get; set; }

        public string UdpServiceName { get; set; }
        public int ShardId { get; set; }
        public int TotalNodes { get; set; }
        public int Position { get; set; }
    }
}
