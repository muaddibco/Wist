using Wist.BlockLattice.Core.Enums;
using Wist.Core;
using Wist.Core.Architecture;

namespace Wist.Node.Core.DPOS
{
    [ServiceContract]
    public interface INodeDposProvidersFactory : IFactory<INodeDposProvider, PacketType>
    {
    }
}
