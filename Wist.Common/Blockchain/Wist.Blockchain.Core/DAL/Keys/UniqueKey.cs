using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Identity;

namespace Wist.Blockchain.Core.DAL.Keys
{
    public class UniqueKey : IDataKey
    {
        public UniqueKey(IKey key)
        {
            IdentityKey = key;
        }

        public IKey IdentityKey { get; set; }
    }
}
