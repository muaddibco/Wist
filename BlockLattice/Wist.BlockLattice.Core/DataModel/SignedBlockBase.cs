using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wist.Core.Identity;

namespace Wist.BlockLattice.Core.DataModel
{
    public abstract class SignedBlockBase : BlockBase
    {
        public IKey Signer { get; set; }
        public byte[] Signature { get; set; }

        public override byte[] NonHeaderBytes => BodyBytes.Concat(Signature).Concat(Signer.Value).ToArray();
    }
}
