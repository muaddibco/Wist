using CommonServiceLocator;
using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Node.Core.Exceptions;
using Wist.Node.Core.Interfaces;

namespace Wist.Node.Core.DPOS
{
    [RegisterDefaultImplementation(typeof(INodeDposProvidersFactory), Lifetime = LifetimeManagement.Singleton)]
    public class NodeDposProvidersFactory : INodeDposProvidersFactory
    {
        private readonly Dictionary<PacketType, Stack<INodeDposProvider>> _nodeDposProviders = new Dictionary<PacketType, Stack<INodeDposProvider>>();

        public NodeDposProvidersFactory(INodeDposProvider[] nodeDposProviders)
        {
            foreach (INodeDposProvider nodeDposProvider in nodeDposProviders)
            {
                if(!_nodeDposProviders.ContainsKey(nodeDposProvider.PacketType))
                {
                    _nodeDposProviders.Add(nodeDposProvider.PacketType, new Stack<INodeDposProvider>());
                    _nodeDposProviders[nodeDposProvider.PacketType].Push(nodeDposProvider);
                }
            }
        }

        public INodeDposProvider Create(PacketType key)
        {
            if(!_nodeDposProviders.ContainsKey(key))
            {
                throw new DposProviderNotSupportedException(key);
            }

            if(_nodeDposProviders[key].Count > 1)
            {
                return _nodeDposProviders[key].Pop();
            }
            else
            {
                INodeDposProvider nodeDposProviderTemp = _nodeDposProviders[key].Pop();
                INodeDposProvider nodeDposProvider = (INodeDposProvider)ServiceLocator.Current.GetInstance(nodeDposProviderTemp.GetType());
                _nodeDposProviders[key].Push(nodeDposProviderTemp);
                return nodeDposProvider;
            }
        }

        public void Utilize(INodeDposProvider obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (!_nodeDposProviders.ContainsKey(obj.PacketType))
            {
                throw new DposProviderNotSupportedException(obj.PacketType);
            }

            _nodeDposProviders[obj.PacketType].Push(obj);
        }
    }
}
