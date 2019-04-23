using System.Collections.Generic;
using Wist.Core.Architecture;

namespace Wist.Node.Core.Rating
{
    /// <summary>
    /// Service for storing information about DPOS votes in memory. 
    /// </summary>
    [ServiceContract]
    public interface IDposService
    {
        /// <summary>
        /// This function reads all existing accounts (of different types) and calculates their contribution to DPOS
        /// </summary>
        void Initialize();

        SortedDictionary<ushort, byte[]> GetTopNodesPublicKeys(int topNumber);
    }
}
