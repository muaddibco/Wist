using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture.Enums;
using Wist.Core.ExtensionMethods;

namespace Wist.Core.Architecture
{
    /// <summary>
    /// Marks type to be regstered within Unity Container
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class RegisterType : Attribute
    {
        /// <summary>
        /// Optional. Type to be used to instanciate the registered Type.
        /// The factory type must implement ITypeFactory and be registered withing the container or be a concrete type.
        /// Should not be used with Implements property
        /// </summary>
        public Type Factory { get; set; }

        /// <summary>
        /// Optional. Registers as implementation of an interface of base class 
        /// Should not be used with Factory property
        /// </summary>
        public Type Implements { get; set; }

        /// <summary>
        /// Optional. Type to be resolved. 
        /// When null, autodetermined from type the attribute is applyed on
        /// </summary>
        public Type TypeToRegister { get; set; }

        /// <summary>
        /// Optional. Object instance to be resolved. 
        /// Must not be used with TypeToRegister
        /// </summary>
        public object InstanceToRegister { get; set; }

        /// <summary>
        /// Allow the type to be overriden
        /// </summary>
        public bool AllowsOverride { get; set; }

        /// <summary>
        /// Instance lifetime.
        /// Defaults to LifetimeManagement.Transient
        /// </summary>
        public LifetimeManagement Lifetime { get; set; }

        /// <summary>
        /// This helps to resolve the corrent instances while resolving
        /// Defaults to RegistrationRole.DefaultImplementation
        /// </summary>
        public RegistrationRole Role { get; set; }

        /// <summary>
        /// This helps to resolve the corrent instances while resolving
        /// Defaults to RegistrationRole.DefaultImplementation
        /// </summary>
        public double ExtensionOrderPriority { get; set; }

        public string ResolvingTypeName
        {
            get
            {
                if (InstanceToRegister != null)
                {
                    return $"Instance of {InstanceToRegister.GetType().Name}";
                }

                if (TypeToRegister != null)
                {
                    return TypeToRegister.Name;
                }

                return string.Empty;
            }
        }

        public string ResolvingTypeFullName
        {
            get
            {
                if (InstanceToRegister != null)
                {
                    return $"Instance of {InstanceToRegister.GetType().FullNameWithAssemblyPath()}";
                }

                if (TypeToRegister != null)
                {
                    return TypeToRegister.FullNameWithAssemblyPath();
                }

                return string.Empty;
            }
        }

        public RegisterType()
        {
            Lifetime = LifetimeManagement.Transient;
            Role = RegistrationRole.DefaultImplementation;
        }

        public override string ToString()
        {
            return $"{Role}: {TypeToRegister.FullName} => {Implements.FullName}; {Lifetime}";
        }
    }
}
