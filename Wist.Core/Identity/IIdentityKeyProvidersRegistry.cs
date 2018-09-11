using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;

namespace Wist.Core.Identity
{
    [ServiceContract]
    public interface IIdentityKeyProvidersRegistry : IRegistry<IIdentityKeyProvider>, IRepository<IIdentityKeyProvider, string>
    {
        IIdentityKeyProvider GetTransactionsIdenityKeyProvider();
    }
}
