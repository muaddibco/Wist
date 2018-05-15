using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.Interfaces
{
    public interface ISignatureSupportSerializer
    {
        ChainType ChainType { get; }

        ushort BlockType { get; }

        byte[] GetBody(SignedBlockBase signedBlockBase);
    }
}
