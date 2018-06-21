using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Wist.BlockLattice.Core.DataModel.Nodes
{
    public class Node : AccountBase
    {
        IPAddress IPAddress { get; set; }

        ulong Votes { get; set; }
    }
}
