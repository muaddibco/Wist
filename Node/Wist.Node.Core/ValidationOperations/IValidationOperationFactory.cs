using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;

namespace Wist.Node.Core.ValidationOperations
{
    [ServiceContract]
    public interface IValidationOperationFactory
    {
        IValidationOperation GetNextOperation(PacketType chainType, IValidationOperation prevOperation = null);

        void Utilize(IValidationOperation operation);
    }
}
