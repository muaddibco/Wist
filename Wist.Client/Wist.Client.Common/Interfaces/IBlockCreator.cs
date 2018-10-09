using Wist.BlockLattice.Core.DataModel;
using Wist.Core;
using Wist.Core.Architecture;

namespace Wist.Client.Common.Interfaces
{
    [ServiceContract]
    public interface IBlockCreator : IRepository<BlockBase, ushort>
    {
    }
}