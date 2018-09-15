using System.Threading;
using Wist.Core.Architecture;

namespace Wist.Node.Core.Synchronization
{
    [ServiceContract]
    public interface ISynchronizationService
    {
        void Initialize(CancellationToken cancellationToken);

        void Start();
    }
}
