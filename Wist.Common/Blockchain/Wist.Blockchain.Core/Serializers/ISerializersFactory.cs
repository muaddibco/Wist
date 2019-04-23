using Wist.Blockchain.Core.DataModel;
using Wist.Core;
using Wist.Core.Architecture;
using Wist.Core.Cryptography;
using Wist.Core.Models;

namespace Wist.Blockchain.Core.Serializers
{
    [ServiceContract]
    public interface ISerializersFactory : IFactory<ISerializer, PacketBase>
    {
    }
}
