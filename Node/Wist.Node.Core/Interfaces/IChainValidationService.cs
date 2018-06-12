using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;

namespace Wist.Node.Core.Interfaces
{
    [ExtensionPoint]
    public interface IChainValidationService
    {
        PacketType ChainType { get; }

        void Initialize(IConsumeValidationReport reportConsensus, CancellationToken cancellationToken);

        void EnrollForConsensus(BlockBase block);

        bool IsBlockEnrolled(BlockBase block);
    }
}
