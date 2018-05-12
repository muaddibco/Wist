using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Wist.BlockLattice.Core.DataModel;
using Wist.Core.Architecture;
using Wist.Node.Core.Model;

namespace Wist.Node.Core.Interfaces
{
    [ServiceContract]
    public interface IConsensusCheckingService
    {
        void EnrollConsensusDecisions(BlockBase block, IEnumerable<ValidationDecision> consensusDecisions);

        void Initialize(CancellationToken ct);
    }
}
