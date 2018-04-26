using CommonServiceLocator;
using CommunicationLibrary.Exceptions;
using CommunicationLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace CommunicationLibrary.Messages.PacketTypeHandlers
{
    [RegisterDefaultImplementation(typeof(IPacketTypeHandlersFactory), Lifetime = LifetimeManagement.Singleton)]
    public class PacketTypeHandlersFactory : IPacketTypeHandlersFactory
    {
        private readonly Dictionary<PacketTypes, Stack<IPacketTypeHandler>> _packetTypeHandlersCache;
        private readonly object _sync = new object();

        public PacketTypeHandlersFactory(IPacketTypeHandler[] packetTypeHandlers)
        {
            _packetTypeHandlersCache = new Dictionary<PacketTypes, Stack<IPacketTypeHandler>>();

            foreach (var packetTypeHandler in packetTypeHandlers)
            {
                if(!_packetTypeHandlersCache.ContainsKey(packetTypeHandler.PacketType))
                {
                    _packetTypeHandlersCache.Add(packetTypeHandler.PacketType, new Stack<IPacketTypeHandler>());
                    _packetTypeHandlersCache[packetTypeHandler.PacketType].Push(packetTypeHandler);
                }
            }
        }

        public IPacketTypeHandler Create(PacketTypes packetType)
        {
            if (!_packetTypeHandlersCache.ContainsKey(packetType))
            {
                throw new NotSupportedPacketTypeHandlerException(packetType);
            }

            lock (_sync)
            {
                IPacketTypeHandler packetTypeHandler = null;

                if (_packetTypeHandlersCache[packetType].Count > 1)
                {
                    packetTypeHandler = _packetTypeHandlersCache[packetType].Pop();
                }
                else
                {
                    IPacketTypeHandler packetTypeHandlerTemplate = _packetTypeHandlersCache[packetType].Pop();
                    packetTypeHandler = (IPacketTypeHandler)Activator.CreateInstance(packetTypeHandlerTemplate.GetType());
                    _packetTypeHandlersCache[packetType].Push(packetTypeHandlerTemplate);
                    _packetTypeHandlersCache[packetType].Push(packetTypeHandler);
                }

                return packetTypeHandler;
            }
        }

        public void Utilize(IPacketTypeHandler packetTypeHandler)
        {
            if (packetTypeHandler == null)
                throw new ArgumentNullException(nameof(packetTypeHandler));

            _packetTypeHandlersCache[packetTypeHandler.PacketType].Push(packetTypeHandler);
        }
    }
}
