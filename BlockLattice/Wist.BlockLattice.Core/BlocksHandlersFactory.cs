using CommonServiceLocator;
using System.Collections.Generic;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Exceptions;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.BlockLattice.Core
{
    [RegisterDefaultImplementation(typeof(IBlocksHandlersFactory), Lifetime = LifetimeManagement.Singleton)]
    public class BlocksHandlersFactory : IBlocksHandlersFactory
    {
        private readonly Dictionary<string, IBlocksHandler> _blocksProcessors;
        private readonly Dictionary<PacketType, HashSet<IBlocksHandler>> _blocksProcessorsRegistered;

        public BlocksHandlersFactory(IBlocksHandler[] blocksProcessors)
        {
            _blocksProcessorsRegistered = new Dictionary<PacketType, HashSet<IBlocksHandler>>();
            _blocksProcessors = new Dictionary<string, IBlocksHandler>();

            foreach (IBlocksHandler blocksProcessor in blocksProcessors)
            {
                if(!_blocksProcessors.ContainsKey(blocksProcessor.Name))
                {
                    _blocksProcessors.Add(blocksProcessor.Name, blocksProcessor);
                }
            }
        }
        public IBlocksHandler GetInstance(string blocksProcessorName)
        {
            if (!_blocksProcessors.ContainsKey(blocksProcessorName))
            {
                throw new BlocksProcessorNotRegisteredException(blocksProcessorName);
            }

            return _blocksProcessors[blocksProcessorName];
        }

        public IEnumerable<IBlocksHandler> GetBulkInstances(PacketType key)
        {
            //TODO: add key check
            return _blocksProcessorsRegistered[key];
        }

        public void RegisterInstance(IBlocksHandler obj)
        {
            if(!_blocksProcessorsRegistered.ContainsKey(obj.PacketType))
            {
                _blocksProcessorsRegistered.Add(obj.PacketType, new HashSet<IBlocksHandler>());
            }

            _blocksProcessorsRegistered[obj.PacketType].Add(obj);
        }
    }
}
