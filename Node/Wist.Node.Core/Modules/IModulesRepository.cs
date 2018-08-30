using Wist.Core;
using Wist.Core.Architecture;

namespace Wist.Node.Core.Modules
{
    [ServiceContract]
    public interface IModulesRepository : IRepository<IModule, string>, IBulkRegistry<IModule>
    {
    }
}
