using Wist.Core;
using Wist.Core.Architecture;

namespace Wist.Node.Core.Common
{
    [ServiceContract]
    public interface IModulesRepository : IRepository<IModule, string>, IBulkRegistry<IModule>
    {
    }
}
