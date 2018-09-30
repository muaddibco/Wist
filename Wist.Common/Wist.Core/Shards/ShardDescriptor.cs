using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Identity;

namespace Wist.Core.Shards
{
    public class ShardDescriptor
    {
        public ShardDescriptor()
        {
            Nodes = new List<IKey>();
        }

        public int Id { get; set; }

        public List<IKey> Nodes { get; set; }
    }
}
