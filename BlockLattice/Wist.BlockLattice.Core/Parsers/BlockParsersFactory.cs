using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel.Enums;
using Wist.BlockLattice.Core.Exceptions;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.BlockLattice.Core.Parsers
{
    [RegisterDefaultImplementation(typeof(IBlockParsersFactory), Lifetime = LifetimeManagement.Singleton)]
    public class BlockParsersFactory : IBlockParsersFactory
    {
        private Dictionary<BlockType, Stack<IBlockParser>> _blockParsersStack;
        private readonly object _sync = new object();

        public BlockParsersFactory(IBlockParser[] blockParsers)
        {
            _blockParsersStack = new Dictionary<BlockType, Stack<IBlockParser>>();

            foreach (IBlockParser blockParser in blockParsers)
            {
                if(!_blockParsersStack.ContainsKey(blockParser.BlockType))
                {
                    _blockParsersStack.Add(blockParser.BlockType, new Stack<IBlockParser>());
                    _blockParsersStack[blockParser.BlockType].Push(blockParser);
                }
            }
        }

        public IBlockParser Create(BlockType blockType)
        {
            if(!_blockParsersStack.ContainsKey(blockType))
            {
                throw new BlockTypeNotSupportedException(blockType);
            }

            lock(_sync)
            {
                IBlockParser blockParser = null;

                if(_blockParsersStack[blockType].Count > 1)
                {
                    blockParser = _blockParsersStack[blockType].Pop();
                }
                else
                {
                    IBlockParser blockParserTemplate = _blockParsersStack[blockType].Pop();
                    blockParser = (IBlockParser)Activator.CreateInstance(blockParserTemplate.GetType());
                    _blockParsersStack[blockType].Push(blockParserTemplate);
                }

                return blockParser;
            }
        }

        public void Utilize(IBlockParser blockParser)
        {
            if (blockParser == null)
            {
                throw new ArgumentNullException(nameof(blockParser));
            }

            if (!_blockParsersStack.ContainsKey(blockParser.BlockType))
            {
                throw new BlockTypeNotSupportedException(blockParser.BlockType);
            }

            _blockParsersStack[blockParser.BlockType].Push(blockParser);
        }
    }
}
