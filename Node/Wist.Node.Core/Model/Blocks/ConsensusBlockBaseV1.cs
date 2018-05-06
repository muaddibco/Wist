using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Node.Core.Model.Blocks
{
    public abstract class ConsensusBlockBaseV1 : ConsensusBlockBase
    {
        public override ushort Version => 1;
    }
}
