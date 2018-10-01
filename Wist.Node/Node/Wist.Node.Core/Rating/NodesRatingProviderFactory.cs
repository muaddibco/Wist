using System.Collections.Generic;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Node.Core.Exceptions;

namespace Wist.Node.Core.Rating
{
    [RegisterDefaultImplementation(typeof(INodesRatingProviderFactory), Lifetime = LifetimeManagement.Singleton)]
    public class NodesRatingProviderFactory : INodesRatingProviderFactory
    {
        private readonly Dictionary<PacketType, INodesRatingProvider> _nodesRatingProviders = new Dictionary<PacketType, INodesRatingProvider>();

        public NodesRatingProviderFactory(INodesRatingProvider[] nodesRatingProviders)
        {
            foreach (INodesRatingProvider nodesRatingProvider in nodesRatingProviders)
            {
                if(!_nodesRatingProviders.ContainsKey(nodesRatingProvider.PacketType))
                {
                    _nodesRatingProviders.Add(nodesRatingProvider.PacketType, nodesRatingProvider);
                }
            }
        }

        public INodesRatingProvider GetInstance(PacketType key)
        {
            if(!_nodesRatingProviders.ContainsKey(key))
            {
                throw new DposProviderNotSupportedException(key);
            }

            return _nodesRatingProviders[key];
        }
    }
}
