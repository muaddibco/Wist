using Wist.Core;
using Wist.Core.Architecture;

namespace Wist.Node.Core.Interfaces
{
    [ServiceContract]
    public interface IRolesRepository : IRepository<IRole, string>, IBulkRepository<IRole>
    {
    }
}
