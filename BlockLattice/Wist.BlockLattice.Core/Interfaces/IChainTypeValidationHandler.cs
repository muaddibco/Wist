using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Wist.Core.Architecture;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Handlers;

namespace Wist.BlockLattice.Core.Interfaces
{
    [ServiceContract]
    public interface IChainTypeValidationHandler
    {
        ChainType ChainType { get; }

        PacketErrorMessage ProcessPacket(byte[] packet);
    }
}
