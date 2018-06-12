using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;
using Wist.Core;
using Wist.Core.Architecture;

namespace Wist.Communication.Interfaces
{
    [ServiceContract]
    public interface IPacketSerializersFactory : IFactory<IPacketSerializer, PacketType, ushort>
    {
    }
}
