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
        public abstract BlockType BlockType { get; }

        public abstract ushort Version { get; }

        public uint BlockOrder { get; set; }
    }
}
