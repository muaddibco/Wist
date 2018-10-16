using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel.Registry.SourceKeys;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel.Registry
{
    public interface ITransactionRegistryBlock
    {
        ulong SyncBlockHeight { get; }

        ITransactionSourceKey TransactionSourceKey { get; }

        PacketType ReferencedPacketType { get; }

        ushort ReferencedBlockType { get; }

        byte[] ReferencedBodyHash { get; }

        byte[] ReferencedTarget { get; }

        Memory<byte> BodyBytes { get; }

        Memory<byte> RawData { get; }
    }
}
