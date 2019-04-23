using Wist.Core.Cryptography;
using Wist.Core.Identity;

namespace Wist.Core.Models
{
    public abstract class UtxoSignedPacketBase : PacketBase
    {
        public IKey KeyImage { get; set; }

        public RingSignature[] Signatures { get; set; }

        public IKey[] PublicKeys { get; set; }
    }
}
