using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel
{
    /// <summary>
    /// All blocks in all types of chains must inherit from this base class
    /// </summary>
    public abstract class BlockBase : Entity
    {
        public abstract PacketType PacketType { get; }

        public abstract ushort BlockType { get; }

        public abstract ushort Version { get; }

        /// <summary>
        /// Bytes of packet (without signature and public key)
        /// </summary>
        public byte[] BodyBytes { get; set; }

        public abstract byte[] NonHeaderBytes { get; }

        /// <summary>
        /// All bytes of packet (without DLE + STX and length)
        /// </summary>
        public byte[] RawData { get; set; }
    }
}
