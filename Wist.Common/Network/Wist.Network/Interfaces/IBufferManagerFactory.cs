using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core;
using Wist.Core.Architecture;

namespace Wist.Network.Interfaces
{
    [ServiceContract]
    public interface IBufferManagerFactory : IFactory<IBufferManager>
    {
    }
}
