using Wist.Core.Exceptions;
using Wist.Core.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Wist.Core.PerformanceCounters
{
    public class CategoryFactory
    {
        public static string GetBaseNameFromCounter(string counterName)
        {
            return $"{counterName}Base";
        }

        public static void UnregisterContract(Type contractType)
        {
            var categoryAttr = contractType.GetCustomAttribute<PerfCounterCategoryAttribute>();
            if (null == categoryAttr)
            {
                throw new InvalidContractException($"Expected interface with a {typeof(PerfCounterCategoryAttribute)} attribute");
            }

            if (PerformanceCounterCategory.Exists(categoryAttr.Name))
            {
                PerformanceCounterCategory.Delete(categoryAttr.Name);
            }
        }

        public static void RegisterContract(Type contractType)
        {
            var categoryAttr = contractType.GetCustomAttribute<PerfCounterCategoryAttribute>();
            if (null == categoryAttr)
            {
                throw new InvalidContractException($"Expected class with a {typeof(PerfCounterCategoryAttribute)} attribute");
            }


            if (PerformanceCounterCategory.Exists(categoryAttr.Name))
            {
                return;
                //throw new CategoryAlreadyExistsException($"Category {categoryAttr.Name} already exists");
            }

            var counterCreationDataCollection = new CounterCreationDataCollection();
            foreach (var prop in contractType.GetProperties())
            {
                var attr = prop.GetCustomAttribute<PerfCounterAttribute>();
                if (null == attr)
                {
                    continue;
                }

                if (prop.GetGetMethod() == null)
                {
                    throw new InvalidContractException($"Property {prop.Name} must have a getter and a setter to be valid.");
                }

                if (prop.PropertyType.BaseType.FullName != typeof(PerformanceCounterBase).FullName)
                {
                    throw new InvalidContractException($"Property {prop.Name} must be of {typeof(PerformanceCounterBase).FullName} type.");
                }

                if (prop.PropertyType.CustomAttributes == null || !prop.PropertyType.CustomAttributes.Any(x => x.AttributeType.FullName == typeof(CounterTypeAttribute).FullName))
                {
                    throw new InvalidContractException($"Property {prop.Name} must have a {typeof(CounterTypeAttribute).FullName} attribute.");
                }

                var counterList = GetCounterCreationData(prop.PropertyType, attr);
                counterCreationDataCollection.AddRange(counterList.ToArray());
            }
            
            PerformanceCounterCategory.Create(categoryAttr.Name, categoryAttr.Help, PerformanceCounterCategoryType.MultiInstance, counterCreationDataCollection);
        }

        private static IEnumerable<CounterCreationData> GetCounterCreationData(Type counterType, PerfCounterAttribute counterDetails)
        {
            var ans = new List<CounterCreationData>();

            var attr = counterType.GetCustomAttribute<CounterTypeAttribute>();
            if (attr == null)
            {
                throw new InvalidContractException($"Missing attribute {typeof(CounterTypeAttribute)} for counter type {counterType}");
            }

            CounterCreationData creationData = new CounterCreationData
            {
                CounterName = counterDetails.Name,
                CounterType = attr.CounterType
            };

            ans.Add(creationData);

            if (creationData.CounterType.HasBaseCounterType())
            {
                CounterCreationData creationDataBase = new CounterCreationData
                {
                    CounterType = creationData.CounterType.BaseCounterType(),
                    CounterName = GetBaseNameFromCounter(counterDetails.Name)
                };
                ans.Add(creationDataBase);
            }

            return ans;
        }
    }
}
