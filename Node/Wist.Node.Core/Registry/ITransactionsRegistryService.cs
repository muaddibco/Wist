using Wist.Core.Architecture;

namespace Wist.Node.Core.Registry
{
    [ServiceContract]
    public interface ITransactionsRegistryService
    {
        void Start();

        void Stop();

        void Initialize();
    }
}
