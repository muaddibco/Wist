using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Blockchain.Core.DAL.Keys
{
    public class BlockTypeKey : IDataKey
    {
        public BlockTypeKey(ushort blockType)
        {
            BlockType = blockType;
        }

        public ushort BlockType { get; set; }
    }

    public class BlockTypeLowHeightKey : BlockTypeKey
    {
        public BlockTypeLowHeightKey(ushort blockType, ulong height)
            : base(blockType)
        {
            Height = height;
        }
        public ulong Height { get; set; }
    }
}
