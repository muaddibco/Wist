using Wist.Communication.Interfaces;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wist.Core.ExtensionMethods;

namespace Wist.Communication.Messages.PacketTypeHandlers
{
    public abstract class PacketTypeHandlerBase : IPacketTypeHandler
    {
        private readonly ILog _log;
        private List<IObserver<PacketErrorMessage>> _packetErrorsObservers;

        public PacketTypeHandlerBase()
        {
            _log = LogManager.GetLogger(GetType());
        }

        public abstract PacketTypes PacketType { get; }

        public abstract Task<PacketErrorMessage> ProcessPacket(byte[] packet);
    }
}
