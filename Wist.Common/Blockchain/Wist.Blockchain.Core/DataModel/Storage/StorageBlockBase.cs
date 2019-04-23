using Wist.Core.Models;

namespace Wist.Blockchain.Core.DataModel.Storage
{
    public abstract class StorageBlockBase : SignedPacketBase
    {
        public override ushort PacketType => (ushort)Enums.PacketType.Storage;
    }
}
