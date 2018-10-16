using System;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;

namespace Wist.BlockLattice.Core.Parsers
{
    [ExtensionPoint]
    public interface IBlockParser
    {
        PacketType PacketType { get; }

        ushort BlockType { get; }

        BlockBase Parse(Memory<byte> source);

        BlockBase ParseBody(byte[] blockBody);
    }
}
