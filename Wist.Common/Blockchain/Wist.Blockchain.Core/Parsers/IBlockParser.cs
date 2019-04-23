using System;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Models;

namespace Wist.Blockchain.Core.Parsers
{
    [ExtensionPoint]
    public interface IBlockParser
    {
        PacketType PacketType { get; }

        ushort BlockType { get; }

        PacketBase Parse(Memory<byte> source);
    }
}
