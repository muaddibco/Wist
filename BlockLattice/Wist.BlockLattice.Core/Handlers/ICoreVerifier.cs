using Wist.BlockLattice.Core.DataModel;
using Wist.Core.Architecture;

namespace Wist.BlockLattice.Core.Handlers
{
    [ExtensionPoint]
    public interface ICoreVerifier
    {
        bool VerifyBlock(BlockBase blockBase);
    }
}
