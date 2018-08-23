using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wist.BlockLattice.Core.DataModel
{
    public abstract class SignedBlockBase : BlockBase
    {
        public byte[] Signature { get; set; }

        public override byte[] NonHeaderBytes => BodyBytes.Concat(Signature).Concat(Key.Value).ToArray();
    }
}
