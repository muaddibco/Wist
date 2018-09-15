using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel.Nodes;
using Wist.Core.Architecture;
using Wist.Core.Identity;

namespace Wist.BlockLattice.Core.Interfaces
{
    [ServiceContract]
    public interface INodesDataService : IDataService<Node, IKey>
    {
    }
}
