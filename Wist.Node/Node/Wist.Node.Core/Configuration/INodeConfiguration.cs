using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Configuration;

namespace Wist.Node.Core.Configuration
{
    public interface INodeConfiguration : IConfigurationSection
    {
        string[] Modules { get; set; }

        string[] CommunicationServices { get; set; }
    }
}
