using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel
{
    /// <summary>
    /// All blocks in all types of chains must inherit from this base class
    /// </summary>
    public abstract class BlockBase
    {
        public abstract ChainType ChainType { get; }

        public abstract ushort BlockType { get; }

        public abstract ushort Version { get; }

        public uint BlockOrder { get; set; }

        /// <summary>
        /// 64? byte value
        /// </summary>
        public byte[] Nonce { get; set; }

        /// <summary>
        /// 64 byte value
        /// </summary>
        public byte[] HashNonce { get; set; }

        /// <summary>
        /// 64 byte value
        /// </summary>
        public byte[] Hash { get; set; }
    }
}
