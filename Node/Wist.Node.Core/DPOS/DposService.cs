using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Node.Core.Interfaces;

namespace Wist.Node.Core.DPOS
{
    [RegisterDefaultImplementation(typeof(IDposService), Lifetime = LifetimeManagement.Singleton)]
    public class DposService : IDposService
    {
        public SortedDictionary<ushort, byte[]> GetTopNodesPublicKeys(int topNumber)
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {
            
        }
    }
}
