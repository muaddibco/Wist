using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Node.Core.Rating
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
