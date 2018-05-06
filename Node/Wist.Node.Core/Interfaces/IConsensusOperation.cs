using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;

namespace Wist.Node.Core.Interfaces
{
    [ExtensionPoint]
    public interface IConsensusOperation
    {
        ChainType ChainType { get; }

        ushort Priority { get; }

        Task<bool> Validate(BlockBase blockBase);
    }
}
