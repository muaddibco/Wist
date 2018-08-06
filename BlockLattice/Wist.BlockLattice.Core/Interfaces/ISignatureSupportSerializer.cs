using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Communication;

namespace Wist.BlockLattice.Core.Interfaces
{
    [ExtensionPoint]
    public interface ISignatureSupportSerializer : IPacketProvider
    {
        PacketType PacketType { get; }

        ushort BlockType { get; }

        void Initialize(SignedBlockBase signedBlockBase);

        void FillBodyAndRowBytes();
    }
}
