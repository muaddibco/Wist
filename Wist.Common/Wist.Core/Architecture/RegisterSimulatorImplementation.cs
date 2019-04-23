using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture.Enums;

namespace Wist.Core.Architecture
{
    /// <summary>
    /// Registers decorated class as simulator implementation of service specified by input argument "Implements" of the attribute constructor.
    /// </summary>
    /// <seealso cref="RegisterType" />
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false, AllowMultiple = true)]
    public class RegisterSimulatorImplementation : RegisterType
    {
        public RegisterSimulatorImplementation(Type implements)
        {
            Implements = implements;
            Lifetime = LifetimeManagement.Transient;
            Role = RegistrationRole.SimulatorImplementation;
            AllowsOverride = false;
        }
    }
}
