using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.Node.Core.Enums;

namespace Wist.Node.Core.Interfaces
{
    public interface IReportConsensus
    {
        void OnReportConsensus(BlockBase block, ConsensusState consensusState);
    }
}
