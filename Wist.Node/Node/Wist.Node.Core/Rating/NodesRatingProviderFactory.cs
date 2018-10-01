using CommonServiceLocator;
using System;
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
        private readonly Dictionary<PacketType, Stack<INodesRatingProvider>> _nodesRatingProviders = new Dictionary<PacketType, Stack<INodesRatingProvider>>();

        public NodesRatingProviderFactory(INodesRatingProvider[] nodesRatingProviders)
        {
            foreach (INodesRatingProvider nodesRatingProvider in nodesRatingProviders)
            {
                if(!_nodesRatingProviders.ContainsKey(nodesRatingProvider.PacketType))
                {
                    _nodesRatingProviders.Add(nodesRatingProvider.PacketType, new Stack<INodesRatingProvider>());
                    _nodesRatingProviders[nodesRatingProvider.PacketType].Push(nodesRatingProvider);
                }
            }
        }

        public INodesRatingProvider Create(PacketType key)
        {
            if(!_nodesRatingProviders.ContainsKey(key))
            {
                throw new DposProviderNotSupportedException(key);
            }

            if(_nodesRatingProviders[key].Count > 1)
            {
                return _nodesRatingProviders[key].Pop();
            }
            else
            {
                INodesRatingProvider nodeDposProviderTemp = _nodesRatingProviders[key].Pop();
                INodesRatingProvider nodeDposProvider = (INodesRatingProvider)ServiceLocator.Current.GetInstance(nodeDposProviderTemp.GetType());
                _nodesRatingProviders[key].Push(nodeDposProviderTemp);
                return nodeDposProvider;
            }
        }

        public void Utilize(INodesRatingProvider obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (!_nodesRatingProviders.ContainsKey(obj.PacketType))
            {
                throw new DposProviderNotSupportedException(obj.PacketType);
            }

            _nodesRatingProviders[obj.PacketType].Push(obj);
        }
    }
}
