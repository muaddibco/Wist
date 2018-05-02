using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Wist.BlockLattice.Core.DataModel;
using Wist.Core.Architecture;

namespace Wist.BlockLattice.Core.Interfaces
{
    [ServiceContract]
    public interface IBlocksProcessor
    {
        void Initialize(CancellationToken ct);
        void ProcessBlock(BlockBase blockBase);
    }
}
