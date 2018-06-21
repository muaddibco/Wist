using System.Collections.Generic;
using System.Linq;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Node.Core.Interfaces;

namespace Wist.Node.Core.Roles
{
    [RegisterDefaultImplementation(typeof(IRolesRepository), Lifetime = LifetimeManagement.Singleton)]
    public class RolesRepository : IRolesRepository
    {
        private readonly Dictionary<string, IRole> _roles;

        public RolesRepository(IRole[] roles)
        {
            _roles = roles.ToDictionary(r => r.Name, r => r);
        }

        public IRole GetInstance(string key)
        {
            //TODO: add key check and dedicated exception
            return _roles[key];
        }
    }

}
