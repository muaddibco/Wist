using CommonServiceLocator;
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Unity;
using Unity.Lifetime;
using Unity.ServiceLocation;
using Wist.Core.Architecture.Enums;
using Wist.Core.Exceptions;
using Wist.Core.ExtensionMethods;

namespace Wist.Core.Architecture.Registration
{
    public class RegistrationManager : IRegistrationManager
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(RegistrationManager));

        private readonly IUnityContainer _container;
        private List<ExtensionPoint> ExtensionPoints { get; set; }
        private List<ServiceContract> ServiceContracts { get; set; }
        private List<RegisterType> TypeRegistrations { get; set; }
        private HashSet<int> RegisteredTypeRegistrations { get; set; }
        private HashSet<int> RegisteredExtentionPoints { get; set; }

        public RunMode CurrentRunMode { get; set; }

        public RegistrationManager(RunMode runMode, IUnityContainer unityContainer)
        {
            _container = unityContainer;
            ExtensionPoints = new List<ExtensionPoint>();
            ServiceContracts = new List<ServiceContract>();
            TypeRegistrations = new List<RegisterType>();
            RegisteredTypeRegistrations = new HashSet<int>();
            RegisteredExtentionPoints = new HashSet<int>();
            CurrentRunMode = runMode;
        }

        public void RegisterExtensionPoint(ExtensionPoint extensionPoint)
        {
            ExtensionPoints.Add(extensionPoint);
        }

        public void RegisterServiceContract(ServiceContract serviceContract)
        {
            ServiceContracts.Add(serviceContract);
        }

        public void RegisterType(RegisterType type)
        {
            switch (type.Role)
            {
                case RegistrationRole.DefaultImplementation:
                    //_logger.Info(_logCategory, "RegistrationsManager: Registering Type '{0}' as default implementation for '{1}' Service, Lifetime = {2}", type.ResolvingTypeName, type.Implements.Name, type.Lifetime);
                    break;
                case RegistrationRole.SimulatorImplementation:
                    //_logger.Info(_logCategory, "RegistrationsManager: Registering Type '{0}' as simulator implementation for '{1}' Service, Lifetime = {2}", type.ResolvingTypeName, type.Implements.Name, type.Lifetime);
                    break;
                case RegistrationRole.Extension:
                    //_logger.Info(_logCategory, "RegistrationsManager: Registering Extension '{0}' implementing '{1}' Extension Point, Lifetime = {2}", type.ResolvingTypeName, type.Implements.Name, type.Lifetime);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type.Role");
            }

            TypeRegistrations.Add(type);
        }

        public void AutoRegisterType<T>()
        {
            AutoRegisterTypeInternal(typeof(T));
        }

        public void AutoRegisterType(Type type)
        {
            AutoRegisterTypeInternal(type);
        }

        public void AutoRegisterAssembly(Assembly assembly)
        {
            var types = from type in assembly.GetTypes()
                        select type;

            foreach (var type in types)
            {
                AutoRegisterTypeInternal(type);
            }
        }

        private void CollectExtensionPoints(Type type)
        {
            var attributes = type.GetAttributeList<ExtensionPoint>();

            foreach (var attribute in attributes)
            {
                if (attribute.Contract == null)
                {
                    attribute.Contract = type;
                }

                RegisterExtensionPoint(attribute);
            }
        }

        private void AutoRegisterTypeInternal(Type type)
        {
            CollectExtensionPoints(type);
            CollectServiceContracts(type);

            CollectRegistrations(type);
        }

        private void CollectServiceContracts(Type type)
        {
            var attributes = type.GetAttributeList<ServiceContract>();

            foreach (var attribute in attributes)
            {
                if (attribute.Contract == null)
                {
                    attribute.Contract = type;
                }

                RegisterServiceContract(attribute);
            }
        }

        public void AutoRegisterUsingMefCatalog(ComposablePartCatalog catalog)
        {
            var mefRegistrator = new TypesRegistratorsManager();

            try
            {
                var registrators = mefRegistrator.GetAllRegistrators(catalog);

                // register
                foreach (var r in registrators)
                {
                    r.RegisterWithResources(this);
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                string message = "Extensions Load Exceptions:\r\n";
                if (ex.LoaderExceptions != null)
                {
                    message = ex.LoaderExceptions.Aggregate(message,
                        (current, loaderException) =>
                            current + $"------\r\n{loaderException}\r\n--------\r\n");
                }

                _log.Fatal(message, ex);

                throw;
            }
        }

        private void CollectRegistrations(Type type)
        {
            List<RegisterType> attributes = type.GetAttributeList<RegisterType>();

            foreach (RegisterType attribute in attributes)
            {
                if (attribute.TypeToRegister == null)
                {
                    attribute.TypeToRegister = type;
                }

                if (attribute.Implements == null)
                {
                    attribute.Implements = type;
                }

                RegisterType(attribute);
            }
        }

        public void SetupServiceLocator()
        {
            UnityServiceLocator unityServiceLocator = new UnityServiceLocator(_container);
            if (!ServiceLocator.IsLocationProviderSet)
                ServiceLocator.SetLocatorProvider(() => unityServiceLocator);
        }

        public void CommitRegistrationsToContainer()
        {
            //RegisterExtensionPointsIntoContainer(_container);
            TypeRegistrations.Sort((t1, t2) => t1.ExtensionOrderPriority.CompareTo(t2.ExtensionOrderPriority));

            AdjustToRunMode();

            CheckNoCircularReferences();

            RegisterTypesIntoContainer(_container);
        }

        private void RegisterExtensionPointsIntoContainer(IUnityContainer container)
        {
            foreach (var extensionPoint in ExtensionPoints)
            {
                if (RegisteredExtentionPoints.Contains(RuntimeHelpers.GetHashCode(extensionPoint)))
                {
                    continue;
                }

                RegisteredExtentionPoints.Add(RuntimeHelpers.GetHashCode(extensionPoint));

                // register enumerable for Extension contract to resolve IEnumerable<Contract>
                Type enumerableType = typeof(IEnumerable<>).MakeGenericType(extensionPoint.Contract);
                Type arrayType = extensionPoint.Contract.MakeArrayType();

                container.RegisterType(enumerableType, arrayType, null, new TransientLifetimeManager());
            }
        }

        private void RegisterTypesIntoContainer(IUnityContainer container)
        {
            foreach (var registerAttribute in TypeRegistrations)
            {
                // skip registered
                if (RegisteredTypeRegistrations.Contains(RuntimeHelpers.GetHashCode(registerAttribute)))
                {
                    continue;
                }

                RegisteredTypeRegistrations.Add(RuntimeHelpers.GetHashCode(registerAttribute));

                // prepare registration
                Type from = registerAttribute.Implements;
                Type to = registerAttribute.TypeToRegister;

                string name = GenerateRegistrationName(to, registerAttribute);
                LifetimeManager lifetime = GenerateLifetimeManager(registerAttribute.Lifetime);

                // register
                if (registerAttribute.InstanceToRegister != null)
                {
                    // registering instance
                    container.RegisterInstance(from, name, registerAttribute.InstanceToRegister, lifetime);
                }
                else
                {
                    // registering type to create
                    container.RegisterType(from, to, name, lifetime);
                }
            }
        }

        private void AdjustToRunMode()
        {
            if (CurrentRunMode == RunMode.Simulator)
            {
                IEnumerable<RegisterType> simulatorRegisterTypes =
                    TypeRegistrations.Where(tr => tr.Role == RegistrationRole.SimulatorImplementation);

                TypeRegistrations.RemoveAll(tr => tr.Role == RegistrationRole.DefaultImplementation && simulatorRegisterTypes.Any(srt => srt.Implements == tr.Implements));
            }
            else
            {
                TypeRegistrations.RemoveAll(tr => tr.Role == RegistrationRole.SimulatorImplementation);
            }
        }

        private void CheckNoCircularReferences()
        {
            Dictionary<Type, HashSet<Type>> typeToConstructorParams = new Dictionary<Type, HashSet<Type>>();

            foreach (var registerAttribute in TypeRegistrations)
            {
                ConstructorInfo[] constructorInfos = registerAttribute.TypeToRegister.GetConstructors();
                HashSet<Type> constructorParamTypes = new HashSet<Type>(constructorInfos.SelectMany(ci => ci.GetParameters().Select(p => p.ParameterType.IsArray ? p.ParameterType.GetElementType() : p.ParameterType)));

                IEnumerable<RegisterType> referencedRegisterTypes = TypeRegistrations.Where(t => constructorParamTypes.Any(p => p == t.Implements));

                foreach (RegisterType referencedRegisterType in referencedRegisterTypes)
                {
                    ConstructorInfo[] referencedConstructorInfos = referencedRegisterType.TypeToRegister.GetConstructors();
                    HashSet<Type> referencedConstructorParamTypes = new HashSet<Type>(referencedConstructorInfos.SelectMany(ci => ci.GetParameters().Select(p => p.ParameterType.IsArray ? p.ParameterType.GetElementType() : p.ParameterType)));

                    if (referencedConstructorParamTypes.Contains(registerAttribute.Implements))
                    {
                        throw new CircularClassReferenceException(registerAttribute.TypeToRegister, referencedRegisterType.TypeToRegister);
                    }
                }
            }
        }

        private LifetimeManager GenerateLifetimeManager(LifetimeManagement lifeManagementEnum)
        {
            switch (lifeManagementEnum)
            {
                case LifetimeManagement.Transient:
                    return new TransientLifetimeManager();
                case LifetimeManagement.TransientPerResolve:
                    return new PerResolveLifetimeManager();
                case LifetimeManagement.Singleton:
                    return new ContainerControlledLifetimeManager();
                case LifetimeManagement.ThreadSingleton:
                    return new PerThreadLifetimeManager();
                default:
                    throw new ArgumentOutOfRangeException(nameof(lifeManagementEnum));
            }
        }

        private string GenerateRegistrationName(Type type, RegisterType registerAttribute)
        {
            switch (registerAttribute.Role)
            {
                case RegistrationRole.DefaultImplementation:
                    return null;
                case RegistrationRole.SimulatorImplementation:
                    return RunMode.Simulator.ToString();
                case RegistrationRole.Extension:
                    return $"{type.FullName}";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static string EncodeDoubleAsSortableString(double d)
        {
            long ieee = BitConverter.DoubleToInt64Bits(d);
            ulong widezero = 0;
            ulong lex = ((ieee < 0) ? widezero : ((~widezero) >> 1)) ^ (ulong)~ieee;
            return lex.ToString("X16");
        }
    }
}
