using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Unity;
using Wist.Core.Architecture.UnityExtensions.Factory;
using Wist.Core.Architecture.UnityExtensions.Monitor;

namespace Wist.Core.Architecture.UnityExtensions
{
    public class ExtendedUnityContainer : UnityContainer
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(ExtendedUnityContainer));

        public Enums.RunMode CurrentResolutionMode { get; set; }
        private readonly Dictionary<Type, List<string>> _registeredNames;

        public ExtendedUnityContainer()
        {
            _log.Info("ExtendedUnityContainer creation");
            _registeredNames = new Dictionary<Type, List<string>>(); // GetNamesDictionaryByReflection();
            this.RegisterInstance(typeof(IUnityContainer), this);

            CurrentResolutionMode = Enums.RunMode.Default;


            this.AddNewExtension<MonitorUnityExtension>();
            //this.AddNewExtension<FactoryUnityExtension>();
            //this.AddNewExtension<ResolutionModeUnityExtension>();
        }

        private Dictionary<Type, List<string>> GetNamesDictionaryByReflection()
        {
            FieldInfo[] fields = typeof(UnityContainer).GetFields(
                         BindingFlags.NonPublic |
                         BindingFlags.Instance);

            var registeredNamesField =
                (from f in fields
                 where f.Name == "registeredNames"
                 select f).FirstOrDefault();

            Debug.Assert(registeredNamesField != null, "unable to retrieve registeredNames field description");

            var registeredNames = registeredNamesField.GetValue(this);

            Debug.Assert(registeredNames != null, "unable to retrieve registeredNames field value");

            fields = registeredNames.GetType().GetFields(
                         BindingFlags.NonPublic |
                         BindingFlags.Instance);


            var registeredKeysField =
                (from f in fields
                 where f.Name == "registeredKeys"
                 select f).FirstOrDefault();

            Debug.Assert(registeredKeysField != null, "unable to retrieve registeredKeys field description");

            var registeredKeys = registeredKeysField.GetValue(registeredNames) as Dictionary<Type, List<string>>;

            Debug.Assert(registeredKeys != null, "unable to retrieve registeredKeys field value");
            return registeredKeys;
        }

        public void SortNames(Type t)
        {
            if (_registeredNames.ContainsKey(t))
            {
                _registeredNames[t].Sort();
            }

            if (t.GetTypeInfo().IsGenericType && _registeredNames.ContainsKey(t.GetGenericTypeDefinition()))
            {
                _registeredNames[t.GetGenericTypeDefinition()].Sort();
            }
        }

        public List<string> GetRegisteredNames(Type t)
        {
            List<string> registeredNames = GetRegisteredNamesSpecific(t);
            if (t.GetTypeInfo().IsGenericType)
            {
                registeredNames = registeredNames.Concat(GetRegisteredNamesSpecific(t.GetGenericTypeDefinition())).ToList();
            }
            registeredNames = registeredNames.Distinct().ToList();

            return registeredNames;
        }

        private List<string> GetRegisteredNamesSpecific(Type t)
        {
            return !_registeredNames.ContainsKey(t) ? new List<string>() : _registeredNames[t];
        }
    }
}
