using Wist.Blockchain.Core.Enums;
using Wist.Core.Models;

namespace Wist.Blockchain.Core.DataModel.Registry
{
    public abstract class RegistryBlockBase : SignedPacketBase
    {
        public override ushort PacketType => (ushort)Enums.PacketType.Registry;
    }
}
