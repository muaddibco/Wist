using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;

namespace Wist.Node.Core.Interfaces
{
    [ServiceContract]
    public interface IConsensusOperationFactory
    {
        IConsensusOperation GetNextOperation(ChainType chainType, IConsensusOperation prevOperation = null);

        void Utilize(IConsensusOperation operation);
    }
}
