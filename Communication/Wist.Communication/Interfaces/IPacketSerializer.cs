using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;

namespace Wist.Communication.Interfaces
{
    [ExtensionPoint]
    public interface IPacketSerializer
    {
        PacketType ChainType { get; }

        ushort BlockType { get; }

        byte[] GetBodyBytes(BlockBase block);
    }
}
