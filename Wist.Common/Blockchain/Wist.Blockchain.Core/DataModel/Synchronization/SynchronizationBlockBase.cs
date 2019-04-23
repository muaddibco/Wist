using System;
using Wist.Blockchain.Core.Enums;

namespace Wist.Blockchain.Core.DataModel.Synchronization
{
    public abstract class SynchronizationBlockBase : LinkedPacketBase
    {
        public override ushort PacketType => (ushort)Enums.PacketType.Synchronization;

        public DateTime ReportedTime { get; set; }
    }
}
