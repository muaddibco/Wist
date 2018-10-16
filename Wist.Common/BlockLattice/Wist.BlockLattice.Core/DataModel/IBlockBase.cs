using System;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel
{
    public interface IBlockBase
    {
        PacketType PacketType { get; }

        ushort Version { get; }

        ushort BlockType { get; }

        /// <summary>
        /// Bytes of packet (without signature and public key)
        /// </summary>
        Memory<byte> BodyBytes { get; set; }

        Memory<byte> NonHeaderBytes { get; set; }

        /// <summary>
        /// All bytes of packet (without DLE + STX and length)
        /// </summary>
        Memory<byte> RawData { get; set; }
    }
}
