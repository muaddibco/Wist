using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;

namespace Wist.BlockLattice.Core.Parsers
{
    public abstract class BlockParserBase : IBlockParser
    {
        public abstract ushort BlockType { get; }

        public abstract ChainType ChainType { get; }

        public abstract void FillBlockBody(BlockBase block, byte[] blockBody);

        public virtual BlockBase Parse(byte[] source)
        {
            BlockBase block;
            using (MemoryStream ms = new MemoryStream(source))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    br.ReadBytes(8); // skip Chain Type, Message Type and Length of Body
                    block = Parse(br);
                }
            }

            return block;
        }

        protected abstract BlockBase Parse(BinaryReader br);
    }
}
