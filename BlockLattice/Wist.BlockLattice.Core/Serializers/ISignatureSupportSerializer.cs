﻿using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Communication;

namespace Wist.BlockLattice.Core.Serializers
{
    [ExtensionPoint]
    public interface ISignatureSupportSerializer : IPacketProvider, ITransactionKeyProvider
    {
        PacketType PacketType { get; }

        ushort BlockType { get; }

        void Initialize(SignedBlockBase signedBlockBase);

        void FillBodyAndRowBytes();
    }
}
