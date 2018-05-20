using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;

namespace Wist.Node.Core.Interfaces
{
    [ExtensionPoint]
    public interface INodeDposProvider
    {
        ChainType ChainType { get; }

        double GetAllContributions(byte[] nodePublicKey);

        void UpdateContribution(BlockBase block);

        void Initialize();
    }
}
