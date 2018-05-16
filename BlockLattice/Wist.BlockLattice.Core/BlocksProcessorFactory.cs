using CommonServiceLocator;
using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Exceptions;
using Wist.BlockLattice.Core.Interfaces;

namespace Wist.BlockLattice.Core
{
    public class BlocksProcessorFactory : IBlocksProcessorFactory
    {
        private readonly Dictionary<string, Stack<IBlocksProcessor>> _blocksProcessorStack;
        public BlocksProcessorFactory(IBlocksProcessor[] blocksProcessors)
        {
            _blocksProcessorStack = new Dictionary<string, Stack<IBlocksProcessor>>();

            foreach (IBlocksProcessor blocksProcessor in blocksProcessors)
            {
                if(!_blocksProcessorStack.ContainsKey(blocksProcessor.Name))
                {
                    _blocksProcessorStack.Add(blocksProcessor.Name, new Stack<IBlocksProcessor>());
                }

                _blocksProcessorStack[blocksProcessor.Name].Push(blocksProcessor);
            }
        }
        public IBlocksProcessor Create(string blocksProcessorName)
        {
            IBlocksProcessor blocksProcessor = null;
            if (!_blocksProcessorStack.ContainsKey(blocksProcessorName))
            {
                throw new BlocksProcessorNotRegisteredException(blocksProcessorName);
            }

            if(_blocksProcessorStack[blocksProcessorName].Count > 1)
            {
                blocksProcessor = _blocksProcessorStack[blocksProcessorName].Pop();
            }
            else
            {
                IBlocksProcessor blocksProcessorTemplate = _blocksProcessorStack[blocksProcessorName].Pop();
                blocksProcessor = (IBlocksProcessor)ServiceLocator.Current.GetInstance(blocksProcessorTemplate.GetType());
                _blocksProcessorStack[blocksProcessorName].Push(blocksProcessorTemplate);
            }

            return blocksProcessor;
        }

        public void Utilize(IBlocksProcessor blocksProcessor)
        {
            if (!_blocksProcessorStack.ContainsKey(blocksProcessor.Name))
            {
                throw new BlocksProcessorNotRegisteredException(blocksProcessor.Name);
            }

            _blocksProcessorStack[blocksProcessor.Name].Push(blocksProcessor);
        }
    }
}
