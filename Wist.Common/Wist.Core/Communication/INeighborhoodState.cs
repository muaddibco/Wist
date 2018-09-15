using System.Collections.Generic;
using Wist.Core.Identity;
using Wist.Core.States;

namespace Wist.Core.Communication
{
    public interface INeighborhoodState : IState
    {
        bool AddNeighbor(IKey key);

        bool RemoveNeighbor(IKey key);

        IEnumerable<IKey> GetAllNeighbors();
    }
}
