using System.Threading.Tasks;
using Wist.Blockchain.Core.DataModel;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Models;

namespace Wist.Node.Core.ValidationOperations
{
    [ExtensionPoint]
    public interface IValidationOperation
    {
        PacketType ChainType { get; }

        ushort Priority { get; }

        Task<bool> Validate(PacketBase blockBase);
    }
}
