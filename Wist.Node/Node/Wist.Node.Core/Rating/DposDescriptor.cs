using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Identity;

namespace Wist.Node.Core.Rating
{
    public class DposDescriptor
    {
        public IKey SourceIdentity { get; set; }
        public IKey TargetIdentity { get; set; }
        public ulong Votes { get; set; }
    }
}
