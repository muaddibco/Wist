using Wist.Blockchain.Core.DataModel;
using Wist.Core.Architecture;
using Wist.Core.Models;

namespace Wist.Blockchain.Core.Handlers
{
    [ExtensionPoint]
    public interface ICoreVerifier
    {
        bool VerifyBlock(PacketBase blockBase);
    }
}
