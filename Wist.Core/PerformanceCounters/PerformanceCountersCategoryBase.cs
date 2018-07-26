using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Exceptions;

namespace Wist.Core.PerformanceCounters
{
    public abstract class PerformanceCountersCategoryBase : IPerformanceCountersCategoryBase
    {
        private readonly IApplicationContext _applicationContext;

        public PerformanceCountersCategoryBase(IApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public abstract string Name { get; }

        public void Initialize()
        {
            CategoryFactory.UnregisterContract(GetType());
            CategoryFactory.RegisterContract(GetType());

            var categoryAttr = GetType().GetCustomAttribute<PerfCounterCategoryAttribute>();
            if (null == categoryAttr)
            {
                throw new InvalidContractException($"Expected class with a {typeof(PerfCounterCategoryAttribute)} attribute");
            }

            foreach (var prop in GetType().GetProperties())
            {
                if (typeof(PerformanceCounterBase).IsAssignableFrom(prop.PropertyType))
                {
                    CounterTypeAttribute counterTypeAttribute = prop.PropertyType.GetCustomAttribute<CounterTypeAttribute>();
                    //ConstructorInfo constructorInfo = prop.PropertyType.GetConstructors().First();
                    PerformanceCounterBase performanceCounterBase = (PerformanceCounterBase)Activator.CreateInstance(prop.PropertyType);
                    performanceCounterBase.Initialize(categoryAttr.Name, prop.Name, _applicationContext.InstanceName, counterTypeAttribute.CounterType);
                    prop.SetValue(this, performanceCounterBase);//, BindingFlags.CreateInstance |BindingFlags.Public |BindingFlags.Instance, null, new object[] { categoryAttr.Name, prop.Name, "Wist" }));
                }
            }
        }
    }
}
