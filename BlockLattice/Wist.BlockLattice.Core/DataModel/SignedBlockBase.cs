using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.BlockLattice.Core.DataModel
{
    public abstract class SignedBlockBase : BlockBase
    {
        public byte[] Signature { get; set; }

        public byte[] PublicKey { get; set; }
    }
}
