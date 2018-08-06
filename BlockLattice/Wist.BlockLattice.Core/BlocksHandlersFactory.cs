using System.Collections.Generic;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Exceptions;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.BlockLattice.Core
{
    [RegisterDefaultImplementation(typeof(IBlocksHandlersRegistry), Lifetime = LifetimeManagement.Singleton)]
    public class BlocksHandlersRegistry : IBlocksHandlersRegistry
    {
        private readonly Dictionary<string, IBlocksHandler> _blocksHandlers;
        private readonly Dictionary<PacketType, HashSet<IBlocksHandler>> _blocksHandlersRegistered;

        public BlocksHandlersRegistry(IBlocksHandler[] blocksProcessors)
        {
            _blocksHandlersRegistered = new Dictionary<PacketType, HashSet<IBlocksHandler>>();
            _blocksHandlers = new Dictionary<string, IBlocksHandler>();

            foreach (IBlocksHandler blocksProcessor in blocksProcessors)
            {
                if(!_blocksHandlers.ContainsKey(blocksProcessor.Name))
                {
                    _blocksHandlers.Add(blocksProcessor.Name, blocksProcessor);
                }
            }
        }
        public IBlocksHandler GetInstance(string blocksProcessorName)
        {
            if (!_blocksHandlers.ContainsKey(blocksProcessorName))
            {
                throw new BlocksProcessorNotRegisteredException(blocksProcessorName);
            }

            return _blocksHandlers[blocksProcessorName];
        }

        public IEnumerable<IBlocksHandler> GetBulkInstances(PacketType key)
        {
            if(!_blocksHandlersRegistered.ContainsKey(key))
            {
                throw new BlockHandlerNotSupportedException(key);
            }

            return _blocksHandlersRegistered[key];
        }

        public void RegisterInstance(IBlocksHandler obj)
        {
            if(!_blocksHandlersRegistered.ContainsKey(obj.PacketType))
            {
                _blocksHandlersRegistered.Add(obj.PacketType, new HashSet<IBlocksHandler>());
            }

            _blocksHandlersRegistered[obj.PacketType].Add(obj);
        }
    }
}
