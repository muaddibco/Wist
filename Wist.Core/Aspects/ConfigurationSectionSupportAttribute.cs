using Unity;
using PostSharp.Aspects;
using PostSharp.Reflection;
using PostSharp.Serialization;
using System;
using System.ComponentModel;
using Wist.Core.Configuration;
using Wist.Core.Exceptions;

namespace Wist.Core.Aspects
{
    [PSerializable]
    public class ConfigurationSectionSupportAttribute : LocationInterceptionAspect
    {
        private string _propertyName;
        private bool _isOptional;

        public override void CompileTimeInitialize(LocationInfo targetLocation, AspectInfo aspectInfo)
        {
            base.CompileTimeInitialize(targetLocation, aspectInfo);
            object[] attrs = targetLocation.PropertyInfo?.GetCustomAttributes(typeof(OptionalAttribute), true);

            _isOptional = (attrs?.Length ?? 0) > 0;

            _propertyName = targetLocation.Name;
        }

        public override bool CompileTimeValidate(LocationInfo locationInfo)
        {
            if (!typeof(ConfigurationSectionBase).IsAssignableFrom(locationInfo.DeclaringType))
                throw new MandatoryInterfaceNotImplementedException(GetType(), typeof(ConfigurationSectionBase), locationInfo.DeclaringType);

            return !"SectionName".Equals(locationInfo.Name) && !"ApplicationContext".Equals(locationInfo.Name) && (locationInfo.PropertyInfo?.GetMethod.IsPublic ?? false) && (locationInfo.PropertyInfo?.SetMethod.IsPublic ?? false) && base.CompileTimeValidate(locationInfo);
        }

        public override void OnGetValue(LocationInterceptionArgs args)
        {
            ConfigurationSectionBase sectionSupportInstance = args.Instance as ConfigurationSectionBase;

            IAppConfig appConfig = sectionSupportInstance.ApplicationContext.Container.Resolve<IAppConfig>();

            string sectionName = sectionSupportInstance.SectionName;
            string key = string.IsNullOrWhiteSpace(sectionName) ? _propertyName : $"{sectionName}:{_propertyName}";

            string sValue = appConfig.GetString(key.ToLower(), !_isOptional);
            object value = null;

            if (args.Location.LocationType.IsArray)
            {
                string[] arrValues = sValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                Array values = Array.CreateInstance(args.Location.LocationType.GetElementType(), arrValues.Length);
                object arrValue;
                int index = 0;

                foreach (string arrSValue in arrValues)
                {
                    if (TryConvertSingleValue(args.Location.LocationType.GetElementType(), arrSValue.Trim(), out arrValue))
                    {
                        values.SetValue(arrValue, index++);
                    }
                    else
                    {
                        throw new ConfigurationParameterValueConversionFailedException(sValue, key, args.Location.LocationType, _propertyName, args.Location.DeclaringType);
                    }
                }

                value = values;
            }
            else
            {
                if (!TryConvertSingleValue(args.Location.LocationType, sValue, out value))
                {
                    throw new ConfigurationParameterValueConversionFailedException(sValue, key, args.Location.LocationType, _propertyName, args.Location.DeclaringType);
                }
            }

            args.SetNewValue(value);

            args.ProceedGetValue();
        }

        private bool TryConvertSingleValue(Type targetType, string sValue, out object value)
        {
            value = null;
            TypeConverter tcFrom = TypeDescriptor.GetConverter(targetType);
            if (!tcFrom.CanConvertFrom(typeof(string)))
            {
                TypeConverter tcTo = TypeDescriptor.GetConverter(typeof(string));

                if (!tcTo.CanConvertTo(targetType))
                {
                    return false;
                }

                try
                {
                    value = tcTo.ConvertTo(sValue, targetType);
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                try
                {
                    value = tcFrom.ConvertFromString(sValue);
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }
    }
}
