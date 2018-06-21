using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Node.Core.Interfaces;

namespace Wist.Node.Core.Roles
{
    [RegisterDefaultImplementation(typeof(IRolesRegistry), Lifetime = LifetimeManagement.Singleton)]
    public class RolesRegistry : IRolesRegistry
    {
        private readonly HashSet<IRole> _roles = new HashSet<IRole>();

        public IEnumerable<IRole> GetBulkInstances()
        {
            return _roles;
        }

        public void RegisterInstance(IRole role)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            _roles.Add(role);
        }
    }

}
