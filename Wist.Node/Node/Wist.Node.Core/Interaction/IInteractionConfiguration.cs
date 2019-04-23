using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Configuration;

namespace Wist.Node.Core.Interaction
{
    public interface IInteractionConfiguration : IConfigurationSection
    {
        int Port { get; set; }
    }
}
