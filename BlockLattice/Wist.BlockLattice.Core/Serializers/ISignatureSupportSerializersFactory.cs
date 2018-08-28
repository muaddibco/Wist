using Wist.BlockLattice.Core.DataModel;
using Wist.Core;
using Wist.Core.Architecture;

namespace Wist.BlockLattice.Core.Serializers
{
    [ServiceContract]
    public interface ISignatureSupportSerializersFactory : IFactory<ISignatureSupportSerializer, SignedBlockBase>
    {
    }
}
