using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel.Nodes;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Identity;

namespace Wist.BlockLattice.SQLite.DataServices
{

    [RegisterDefaultImplementation(typeof(INodesDataService), Lifetime = LifetimeManagement.Singleton)]
    public class NodesDataService : INodesDataService
    {
        public void Add(Node item)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Node> GetAll()
        {
            throw new NotImplementedException();
        }
    }
}
