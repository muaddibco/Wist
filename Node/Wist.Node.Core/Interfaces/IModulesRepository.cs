using Wist.Core;
using Wist.Core.Architecture;

namespace Wist.Node.Core.Interfaces
{
    [ServiceContract]
    public interface IModulesRepository : IRepository<IModule, string>, IBulkRepository<IModule>
    {
    }
}
