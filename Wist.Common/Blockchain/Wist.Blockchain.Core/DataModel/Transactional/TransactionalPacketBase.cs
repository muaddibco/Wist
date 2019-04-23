using Wist.Core.Models;

namespace Wist.Blockchain.Core.DataModel.Transactional
{
    public abstract class TransactionalPacketBase : SignedPacketBase
    {
        public override ushort PacketType => (ushort)Enums.PacketType.Transactional;

        /// <summary>
        /// Up to date funds at last transactional block
        /// </summary>
        public ulong UptodateFunds { get; set; }
    }
}
