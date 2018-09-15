using System.Net;

namespace Wist.BlockLattice.Core.DataModel.Nodes
{
    public class Node : AccountBase
    {
        public IPAddress IPAddress { get; set; }

        public NodeRole NodeRole { get; set; }
    }
}
