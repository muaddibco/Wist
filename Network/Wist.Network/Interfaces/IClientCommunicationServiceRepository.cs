using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core;
using Wist.Core.Architecture;

namespace Wist.Communication.Interfaces
{
    [ServiceContract]
    public interface IClientCommunicationServiceRepository : IRepository<ICommunicationService, string>
    {
    }
}
