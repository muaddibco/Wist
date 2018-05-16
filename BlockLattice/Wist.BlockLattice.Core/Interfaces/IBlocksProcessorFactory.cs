using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core;
using Wist.Core.Architecture;

namespace Wist.BlockLattice.Core.Interfaces
{
    [ServiceContract]
    public interface IBlocksProcessorFactory : IFactory<IBlocksProcessor, string>
    {
    }
}
