using System.Net;
using Wist.Core.Identity;

namespace Wist.Blockchain.Core.DataModel.Nodes
{
    public class Node : AccountBase
    {
        public IKey Key { get; set; }

        public IPAddress IPAddress { get; set; }

        public NodeRole NodeRole { get; set; }
    }
}
