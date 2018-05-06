using CommonServiceLocator;
using PostSharp.Aspects;
using PostSharp.Reflection;
using PostSharp.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Wist.Core.Configuration;
using Wist.Core.Exceptions;

namespace Wist.Core.Aspects
{
    [PSerializable]
    public class ConfigurationSectionSupportAttribute : LocationInterceptionAspect
    {
        private string _propertyName;

        public override void CompileTimeInitialize(LocationInfo targetLocation, AspectInfo aspectInfo)
        {
            base.CompileTimeInitialize(targetLocation, aspectInfo);

            _propertyName = targetLocation.Name;
        }

        public override bool CompileTimeValidate(LocationInfo locationInfo)
        {
            if (!typeof(IConfigurationSection).IsAssignableFrom(locationInfo.DeclaringType))
                throw new MandatoryInterfaceNotImplementedException(GetType(), typeof(IConfigurationSection), locationInfo.DeclaringType);

            return !"SectionName".Equals(locationInfo.Name) && (locationInfo.PropertyInfo?.GetMethod.IsPublic ?? false) && (locationInfo.PropertyInfo?.SetMethod.IsPublic ?? false) && base.CompileTimeValidate(locationInfo);
        }

        public override void OnGetValue(LocationInterceptionArgs args)
        {
            IAppConfig appConfig = ServiceLocator.Current.GetInstance<IAppConfig>();

            IConfigurationSection sectionSupportInstance = args.Instance as IConfigurationSection;

            string sectionName = sectionSupportInstance.SectionName;
            string key = string.IsNullOrWhiteSpace(sectionName) ? _propertyName : $"{sectionName}:{_propertyName}";

            string sValue = appConfig.GetString(key.ToLower());
            object value = null;

            TypeConverter tcFrom = TypeDescriptor.GetConverter(args.Location.LocationType);
            if (!tcFrom.CanConvertFrom(typeof(string)))
            {
                TypeConverter tcTo = TypeDescriptor.GetConverter(typeof(string));

                if (!tcTo.CanConvertTo(args.Location.LocationType))
                {
                    throw new ConfigurationParameterValueConversionFailedException(sValue, key, args.Location.LocationType, _propertyName, args.Location.DeclaringType);
                }

                try
                {
                    value = tcTo.ConvertTo(sValue, args.Location.LocationType);
                }
                catch (Exception ex)
                {
                    throw new ConfigurationParameterValueConversionFailedException(sValue, key, args.Location.LocationType, _propertyName, args.Location.DeclaringType, ex);
                }
            }
            else
            {
                try
                {
                    value = tcFrom.ConvertFromString(sValue);
                }
                catch (Exception ex)
                {
                    throw new ConfigurationParameterValueConversionFailedException(sValue, key, args.Location.LocationType, _propertyName, args.Location.DeclaringType, ex);
                }
            }

            args.SetNewValue(value);

            args.ProceedGetValue();
        }
    }
}
