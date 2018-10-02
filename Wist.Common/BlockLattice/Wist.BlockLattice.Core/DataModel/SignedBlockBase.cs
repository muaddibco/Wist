using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wist.Core.Identity;

namespace Wist.BlockLattice.Core.DataModel
{
    public abstract class SignedBlockBase : BlockBase
    {
        public IKey Signer { get; set; }
        public Memory<byte> Signature { get; set; }

        //public override Memory<byte> NonHeaderBytes => System.Buffers.MemoryPool<byte>.Shared.Rent(). BodyBytes.Concat(Signature).Concat(Signer.Value).ToArray();
    }
}
