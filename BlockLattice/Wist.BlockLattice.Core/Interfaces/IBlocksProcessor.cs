using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;

namespace Wist.BlockLattice.Core.Interfaces
{
    [ExtensionPoint]
    public interface IBlocksProcessor
    {
        string Name { get; }

        PacketType PacketType { get; }

        void Initialize(CancellationToken ct);
        void ProcessBlock(BlockBase blockBase);
    }
}
