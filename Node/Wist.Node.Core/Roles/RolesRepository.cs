using System;
using System.Collections.Generic;
using System.Text;
using Wist.Node.Core.Exceptions;
using Wist.Node.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using System.Collections.ObjectModel;

namespace Wist.Node.Core.Roles
{
    [RegisterDefaultImplementation(typeof(IRolesRepository), Lifetime = LifetimeManagement.Singleton)]
    public class RolesRepository : IRolesRepository
    {
        private readonly Dictionary<string, IRole> _roles;
        private readonly List<IRole> _selectedRoles;

        public RolesRepository(IRole[] roles)
        {
            _roles = new Dictionary<string, IRole>();

            foreach (IRole role in roles)
            {
                if(!_roles.ContainsKey(role.Name))
                {
                    _roles.Add(role.Name, role);
                }
            }

            _selectedRoles = new List<IRole>();
        }

        public IEnumerable<IRole> GetBulkInstances()
        {
            return new ReadOnlyCollection<IRole>(_selectedRoles);
        }

        public IRole GetInstance(string key)
        {
            if(!_roles.ContainsKey(key))
            {
                throw new RoleNotSupportedException(key);
            }

            return _roles[key];
        }

        public void RegisterInstance(IRole role)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            _selectedRoles.Add(role);
        }
    }
}
