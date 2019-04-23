using System.Threading;
using Wist.Core.Architecture;

namespace Wist.Core.Modularity
{
    [ExtensionPoint]
    public interface IModule
    {
        bool IsInitialized { get; }

        string Name { get; }

        void Initialize(CancellationToken ct);

        void Start();
    }
}
