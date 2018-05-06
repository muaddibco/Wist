using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;

namespace Wist.Node.Core.Interfaces
{
    [ServiceContract]
    public interface IChainConsensusServiceManager
    {
        IChainConsensusService GetChainConsensysService(ChainType chainType);
    }
}
