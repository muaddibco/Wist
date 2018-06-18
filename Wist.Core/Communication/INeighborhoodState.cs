using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Models;
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
