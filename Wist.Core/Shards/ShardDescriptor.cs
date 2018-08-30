using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Identity;

namespace Wist.Core.Shards
{
    public class ShardDescriptor
    {
        public int Id { get; set; }

        public List<IKey> Nodes { get; set; }
    }
}
