using CommonServiceLocator;
using System.Collections.Generic;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Exceptions;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.BlockLattice.Core
{
    [RegisterDefaultImplementation(typeof(IBlocksProcessorFactory), Lifetime = LifetimeManagement.Singleton)]
    public class BlocksProcessorFactory : IBlocksProcessorFactory
    {
        private readonly Dictionary<string, IBlocksProcessor> _blocksProcessors;
        private readonly Dictionary<PacketType, HashSet<IBlocksProcessor>> _blocksProcessorsRegistered;

        public BlocksProcessorFactory(IBlocksProcessor[] blocksProcessors)
        {
            _blocksProcessorsRegistered = new Dictionary<PacketType, HashSet<IBlocksProcessor>>();
            _blocksProcessors = new Dictionary<string, IBlocksProcessor>();

            foreach (IBlocksProcessor blocksProcessor in blocksProcessors)
            {
                if(!_blocksProcessors.ContainsKey(blocksProcessor.Name))
                {
                    _blocksProcessors.Add(blocksProcessor.Name, blocksProcessor);
                }
            }
        }
        public IBlocksProcessor GetInstance(string blocksProcessorName)
        {
            if (!_blocksProcessors.ContainsKey(blocksProcessorName))
            {
                throw new BlocksProcessorNotRegisteredException(blocksProcessorName);
            }

            return _blocksProcessors[blocksProcessorName];
        }

        public IEnumerable<IBlocksProcessor> GetBulkInstances(PacketType key)
        {
            //TODO: add key check
            return _blocksProcessorsRegistered[key];
        }

        public void RegisterInstance(IBlocksProcessor obj)
        {
            if(!_blocksProcessorsRegistered.ContainsKey(obj.PacketType))
            {
                _blocksProcessorsRegistered.Add(obj.PacketType, new HashSet<IBlocksProcessor>());
            }

            _blocksProcessorsRegistered[obj.PacketType].Add(obj);
        }
    }
}
