using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Communication;

namespace Wist.Node.Core.Registry
{
    public interface IRegistryGroupState : INeighborhoodState
    {
        int Round { get; set; }
    }
}
