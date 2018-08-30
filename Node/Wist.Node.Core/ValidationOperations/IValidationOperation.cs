using System.Threading.Tasks;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;

namespace Wist.Node.Core.ValidationOperations
{
    [ExtensionPoint]
    public interface IValidationOperation
    {
        PacketType ChainType { get; }

        ushort Priority { get; }

        Task<bool> Validate(BlockBase blockBase);
    }
}
