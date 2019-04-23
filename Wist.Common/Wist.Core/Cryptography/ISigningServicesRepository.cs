using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;

namespace Wist.Core.Cryptography
{
    [ServiceContract]
    public interface ISigningServicesRepository : IRepository<ISigningService, string>
    {
    }
}
