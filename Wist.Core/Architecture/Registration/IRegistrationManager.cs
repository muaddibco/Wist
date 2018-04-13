using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Reflection;
using System.Text;
using Wist.Core.Architecture.Enums;

namespace Wist.Core.Architecture.Registration
{
    public interface IRegistrationManager
    {
        RunMode CurrentRunMode { get; set; }
        void RegisterExtensionPoint(ExtensionPoint extensionPoint);
        void RegisterServiceContract(ServiceContract serviceContract);
        void RegisterType(RegisterType type);
        void AutoRegisterType<T>();
        void AutoRegisterType(Type type);
        void AutoRegisterAssembly(Assembly assembly);
        void AutoRegisterUsingMefCatalog(ComposablePartCatalog catalog);

        void SetupServiceLocator();
        void CommitRegistrationsToContainer();
    }
}
