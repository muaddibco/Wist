using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Wist.Core.Architecture;

namespace CommunicationLibrary.Interfaces
{
    [ServiceContract]
    public interface ICommunicationProvisioningProvider
    {
        void UpdateAllowedEndpoints(IPEndPoint[] allowedEndpoints);
    }
}
