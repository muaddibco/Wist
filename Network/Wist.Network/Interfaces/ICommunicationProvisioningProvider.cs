using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Wist.Core.Architecture;

namespace Wist.Network.Interfaces
{
    [ServiceContract]
    public interface ICommunicationProvisioningProvider
    {
        void UpdateAllowedEndpoints(IPEndPoint[] allowedEndpoints);
    }
}
