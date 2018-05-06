using CommonServiceLocator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Exceptions;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.BlockLattice.Core.Handlers
{
    [RegisterDefaultImplementation(typeof(IChainTypeHandlersFactory), Lifetime = LifetimeManagement.Singleton)]
    public class PacketTypeHandlersFactory : IChainTypeHandlersFactory
    {
        private readonly Dictionary<ChainType, Stack<IChainTypeHandler>> _packetTypeHandlersCache;
        private readonly object _sync = new object();

        public PacketTypeHandlersFactory(IChainTypeHandler[] packetTypeHandlers)
        {
            _packetTypeHandlersCache = new Dictionary<ChainType, Stack<IChainTypeHandler>>();

            foreach (var packetTypeHandler in packetTypeHandlers)
            {
                if(!_packetTypeHandlersCache.ContainsKey(packetTypeHandler.ChainType))
                {
                    _packetTypeHandlersCache.Add(packetTypeHandler.ChainType, new Stack<IChainTypeHandler>());
                    _packetTypeHandlersCache[packetTypeHandler.ChainType].Push(packetTypeHandler);
                }
            }
        }

        public IChainTypeHandler Create(ChainType packetType)
        {
            if (!_packetTypeHandlersCache.ContainsKey(packetType))
            {
                throw new NotSupportedPacketTypeHandlerException(packetType);
            }

            lock (_sync)
            {
                IChainTypeHandler packetTypeHandler = null;

                if (_packetTypeHandlersCache[packetType].Count > 1)
                {
                    packetTypeHandler = _packetTypeHandlersCache[packetType].Pop();
                }
                else
                {
                    IChainTypeHandler packetTypeHandlerTemplate = _packetTypeHandlersCache[packetType].Pop();
                    packetTypeHandler = (IChainTypeHandler)Activator.CreateInstance(packetTypeHandlerTemplate.GetType());
                    _packetTypeHandlersCache[packetType].Push(packetTypeHandlerTemplate);
                }

                return packetTypeHandler;
            }
        }

        public void Utilize(IChainTypeHandler packetTypeHandler)
        {
            if (packetTypeHandler == null)
                throw new ArgumentNullException(nameof(packetTypeHandler));

            if (!_packetTypeHandlersCache.ContainsKey(packetTypeHandler.ChainType))
            {
                throw new NotSupportedPacketTypeHandlerException(packetTypeHandler.ChainType);
            }

            _packetTypeHandlersCache[packetTypeHandler.ChainType].Push(packetTypeHandler);
        }
    }
}
