using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture.Enums;

namespace Wist.Core.Architecture
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false, AllowMultiple = true)]
    public class RegisterExtension : RegisterType
    {
        public RegisterExtension(Type implements, ExtensionOrderPriorities priority = ExtensionOrderPriorities.Normal)
        {
            Implements = implements;
            Lifetime = LifetimeManagement.Transient;
            Role = RegistrationRole.Extension;
            AllowsOverride = true;
            ExtensionOrderPriority = (int)priority;
        }

        public RegisterExtension(Type implements, double customPriority)
        {
            Implements = implements;
            Lifetime = LifetimeManagement.Transient;
            Role = RegistrationRole.Extension;
            AllowsOverride = true;
            ExtensionOrderPriority = customPriority;
        }
    }
}
