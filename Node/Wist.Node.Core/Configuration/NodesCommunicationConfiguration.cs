using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Configuration;

namespace Wist.Node.Core.Configuration
{
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class NodesCommunicationConfiguration : CommunicationConfigurationBase
    {
        public const string SECTION_NAME = "nodesCommunication";
        public override string SectionName => SECTION_NAME;
    }
}
