using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core;
using Wist.Core.Architecture;

namespace CommunicationLibrary.Interfaces
{
    [ServiceContract]
    public interface IPacketTypeHandlersFactory : IFactory<IPacketTypeHandler, PacketTypes>
    {
    }
}
