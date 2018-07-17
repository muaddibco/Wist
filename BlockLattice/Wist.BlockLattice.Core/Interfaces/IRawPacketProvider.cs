using Wist.BlockLattice.Core.DataModel;
using Wist.Core.Architecture;
using Wist.Core.Communication;

namespace Wist.BlockLattice.Core.Interfaces
{
    [ExtensionPoint]
    public interface IRawPacketProvider : IPacketProvider
    {
        void Initialize(BlockBase blockBase);
    }
}
