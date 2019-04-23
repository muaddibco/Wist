using System;
using System.Collections.Generic;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using System.Collections.ObjectModel;
using Wist.Core.Exceptions;

namespace Wist.Core.Modularity
{
    [RegisterDefaultImplementation(typeof(IModulesRepository), Lifetime = LifetimeManagement.Singleton)]
    public class ModulesRepository : IModulesRepository
    {
        private readonly Dictionary<string, IModule> _roles;
        private readonly List<IModule> _selectedRoles;

        public ModulesRepository(IModule[] roles)
        {
            _roles = new Dictionary<string, IModule>();

            foreach (IModule role in roles)
            {
                if(!_roles.ContainsKey(role.Name))
                {
                    _roles.Add(role.Name, role);
                }
            }

            _selectedRoles = new List<IModule>();
        }

        public IEnumerable<IModule> GetBulkInstances()
        {
            return new ReadOnlyCollection<IModule>(_selectedRoles);
        }

        public IModule GetInstance(string key)
        {
            if(!_roles.ContainsKey(key))
            {
                throw new RoleNotSupportedException(key);
            }

            return _roles[key];
        }

        public void RegisterInstance(IModule role)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            _selectedRoles.Add(role);
        }
    }
}
