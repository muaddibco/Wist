using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel.Registry.SourceKeys;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel.Registry
{
    public interface ITransactionRegistryBlock
    {
        ITransactionSourceKey TransactionSourceKey { get; }

        PacketType ReferencedPacketType { get; set; }

        ushort ReferencedBlockType { get; set; }

        byte[] ReferencedBodyHash { get; set; }

        byte[] ReferencedTarget { get; set; }
    }
}
