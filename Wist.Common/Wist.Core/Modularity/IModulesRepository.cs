using Wist.Core.Architecture;

namespace Wist.Core.Modularity
{
    [ServiceContract]
    public interface IModulesRepository : IRepository<IModule, string>, IBulkRegistry<IModule>
    {
    }
}
