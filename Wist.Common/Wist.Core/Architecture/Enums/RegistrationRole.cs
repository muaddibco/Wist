using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Core.Architecture.Enums
{
    public enum RegistrationRole
    {
        /// <summary>
        /// Represents default implementation, can be only one. Resolved by Resolve() method.
        /// </summary>
        DefaultImplementation,

        /// <summary>
        /// Represents implementation that will be loaded when running in Simulator mode, can be only one. Resolved by Resolve() method
        /// </summary>
        SimulatorImplementation,

        /// <summary>
        /// Adds this implementation top list resolved using ResolveAll, can be multiple. Resolved by ResolveAll() method.
        /// </summary>
        Extension
    }
}
