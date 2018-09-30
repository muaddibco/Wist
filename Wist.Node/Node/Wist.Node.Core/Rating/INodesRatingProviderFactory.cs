using Wist.BlockLattice.Core.Enums;
using Wist.Core;
using Wist.Core.Architecture;

namespace Wist.Node.Core.Rating
{
    [ServiceContract]
    public interface INodesRatingProviderFactory : IFactory<INodesRatingProvider, PacketType>
    {
    }
}
