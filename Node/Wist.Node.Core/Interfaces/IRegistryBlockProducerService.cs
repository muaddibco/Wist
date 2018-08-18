using Wist.Core.Architecture;

namespace Wist.Node.Core.Interfaces
{
    [ServiceContract]
    public interface IRegistryBlockProducerService
    {
        void Start();

        void Stop();

        void Initialize();
    }
}
