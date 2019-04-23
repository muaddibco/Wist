using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Exceptions;
using Wist.Core.Logging;

namespace Wist.Core.PerformanceCounters
{
    public abstract class PerformanceCountersCategoryBase : IPerformanceCountersCategoryBase
    {
        private readonly IApplicationContext _applicationContext;
        private readonly ILogger _logger;

        public PerformanceCountersCategoryBase(IApplicationContext applicationContext, ILoggerService loggerService)
        {
            _applicationContext = applicationContext;
            _logger = loggerService.GetLogger(GetType().Name);
        }

        public abstract string Name { get; }

        public void Initialize()
        {
            var categoryAttr = GetType().GetCustomAttribute<PerfCounterCategoryAttribute>();
            if (null == categoryAttr)
            {
                throw new InvalidContractException($"Expected class with a {typeof(PerfCounterCategoryAttribute)} attribute");
            }

            foreach (var prop in GetType().GetProperties())
            {
                try
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
                catch (Exception ex)
                {
                    FailedToInitializeCounterException exception = new FailedToInitializeCounterException(prop.Name, categoryAttr.Name, ex);

                    _logger.Warning(exception.ToString());
                }
            }
        }

        public void Setup()
        {
            CategoryFactory.UnregisterContract(GetType());
            CategoryFactory.RegisterContract(GetType());
        }
    }
}
