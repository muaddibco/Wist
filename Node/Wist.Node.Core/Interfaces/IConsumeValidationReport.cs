using System.Collections.Generic;
using Wist.BlockLattice.Core.DataModel;
using Wist.Node.Core.Model;

namespace Wist.Node.Core.Interfaces
{
    public interface IConsumeValidationReport
    {
        void OnValidationReport(BlockBase block, IEnumerable<ValidationDecision> consensusDecisions);
    }
}
