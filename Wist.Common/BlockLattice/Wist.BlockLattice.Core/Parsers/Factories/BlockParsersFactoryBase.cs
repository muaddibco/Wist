using System;
using System.Collections.Generic;
using System.Linq;
using Unity;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Exceptions;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.BlockLattice.Core.Parsers.Factories
{
    public abstract class BlockParsersRepositoryBase : IBlockParsersRepository
    {
        protected Dictionary<ushort, IBlockParser> _blockParsers;

        public BlockParsersRepositoryBase(IBlockParser[] blockParsers)
        {
            _blockParsers = new Dictionary<ushort, IBlockParser>();

            foreach (IBlockParser blockParser in blockParsers.Where(bp => bp.PacketType == PacketType))
            {
                if (!_blockParsers.ContainsKey(blockParser.BlockType))
                {
                    _blockParsers.Add(blockParser.BlockType, blockParser);
                }
            }
        }

        public abstract PacketType PacketType { get; }

        public IBlockParser GetInstance(ushort blockType)
        {
            if (!_blockParsers.ContainsKey(blockType))
            {
                throw new BlockTypeNotSupportedException(blockType, PacketType);
            }

            return _blockParsers[blockType];
        }
    }
}
