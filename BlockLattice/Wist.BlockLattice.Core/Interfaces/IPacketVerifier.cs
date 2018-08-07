using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Wist.Core.Architecture;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Handlers;
using Wist.Core.Synchronization;
using Wist.BlockLattice.Core.DataModel;

namespace Wist.BlockLattice.Core.Interfaces
{
    [ExtensionPoint]
    public interface IPacketVerifier
    {
        PacketType PacketType { get; }

        bool ValidatePacket(BlockBase block);
    }
}
