using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture.Enums;

namespace Wist.Core.Architecture
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false, AllowMultiple = true)]
    public class RegisterWithFactory : RegisterType
    {
        public RegisterWithFactory(Type factoryType)
        {
            Factory = factoryType;
            Lifetime = LifetimeManagement.Transient;
            Role = RegistrationRole.DefaultImplementation;
            AllowsOverride = true;
        }
    }
}
