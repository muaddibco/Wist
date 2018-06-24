using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;

namespace Wist.Core.Identity
{
    [ExtensionPoint]
    public interface IIdentityKeyProvider
    {
        string Name { get; }

        IKey GetKey(byte[] keyBytes);
    }
}
