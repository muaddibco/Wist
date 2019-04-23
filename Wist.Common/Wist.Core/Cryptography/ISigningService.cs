using Wist.Core.Architecture;
using Wist.Core.Identity;
using Wist.Core.Models;

namespace Wist.Core.Cryptography
{
    [ExtensionPoint]
    public interface ISigningService
    {
        string Name { get; }

        IKey[] PublicKeys { get; }

        bool Verify(IPacket packet);

        void Initialize(params byte[][] secretKeys);

        void Sign(IPacket packet, object args = null);
    }
}
