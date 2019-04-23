using System;
using Wist.Core.Identity;

namespace Wist.Core.Models
{
    public abstract class SignedPacketBase : PacketBase
    {
        public ulong BlockHeight { get; set; }

        public IKey Signer { get; set; }

        public Memory<byte> Signature { get; set; }
    }
}
