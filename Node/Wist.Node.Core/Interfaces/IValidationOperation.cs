using System.Threading.Tasks;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;

namespace Wist.Node.Core.Interfaces
{
    [ExtensionPoint]
    public interface IValidationOperation
    {
        ChainType ChainType { get; }

        ushort Priority { get; }

        Task<bool> Validate(BlockBase blockBase);
    }
}
