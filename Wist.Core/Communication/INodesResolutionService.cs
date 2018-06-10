using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Models;

namespace Wist.Core.Communication
{
    [ServiceContract]
    public interface INodesResolutionService
    {
        void Initialize();

        IPAddress ResolveNodeAddress(IKey key);
    }
}
