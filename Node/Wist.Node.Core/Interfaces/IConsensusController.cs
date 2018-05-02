using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.Core.Architecture;

namespace Wist.Node.Core.Interfaces
{
    [ServiceContract]
    public interface IConsensusController
    {
        void ProcessBlock(BlockBase block);

    }
}
