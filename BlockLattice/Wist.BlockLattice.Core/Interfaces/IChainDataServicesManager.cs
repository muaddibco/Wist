using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;

namespace Wist.BlockLattice.Core.Interfaces
{
    [ServiceContract]
    public interface IChainDataServicesManager
    {
        IChainDataService GetChainDataService(PacketType chainType);
    }
}
