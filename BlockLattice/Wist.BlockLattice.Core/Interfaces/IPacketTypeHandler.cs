using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Wist.Core.Architecture;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Handlers;
using Wist.Core.Synchronization;

namespace Wist.BlockLattice.Core.Interfaces
{
    [ServiceContract]
    public interface IPacketTypeHandler
    {
        PacketType ChainType { get; }

        PacketErrorMessage ValidatePacket(byte[] packet);

        IBlockParsersFactory BlockParsersFactory { get; }
    }
}
