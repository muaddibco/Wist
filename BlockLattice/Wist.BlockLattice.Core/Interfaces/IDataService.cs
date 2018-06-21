using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel;

namespace Wist.BlockLattice.Core.Interfaces
{
    public interface IDataService<T> where T : Entity
    {
        void Add(T item);

        IEnumerable<T> GetAll();
    }
}
