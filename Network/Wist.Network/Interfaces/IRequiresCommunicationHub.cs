using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Network.Interfaces
{
    public interface IRequiresCommunicationHub
    {
        void RegisterCommunicationHub(IServerCommunicationService communicationHub);
    }
}
