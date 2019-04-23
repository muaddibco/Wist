using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Wist.Core.Identity;

namespace Wist.Core.Communication
{
    public class NodeAddress
    {
        public NodeAddress(IKey key, IPAddress ipAddress)
        {
            Key = key;
            IP = ipAddress;
        }

        public IKey Key { get; set; }

        public IPAddress IP { get; set; }
    }
}
