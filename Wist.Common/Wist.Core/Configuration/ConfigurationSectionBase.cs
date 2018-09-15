using System;
using Unity;
using Wist.Core.Architecture;
using System.Reflection;
using System.ComponentModel;
using Wist.Core.Exceptions;

namespace Wist.Core.Configuration
{
    public abstract class ConfigurationSectionBase : IConfigurationSection
    {
        private readonly IAppConfig _appConfig;

        public ConfigurationSectionBase(IApplicationContext applicationContext, string sectionName)
        {
            SectionName = sectionName;

            _appConfig = applicationContext.Container.Resolve<IAppConfig>();
        }

        public string SectionName { get; }

        public void Initialize()
        {
            PropertyInfo[] propertyInfos = GetType().GetProperties();

            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                if (propertyInfo.Name == nameof(SectionName))
                    continue;

                SetPropertyValue(propertyInfo);
            }
        }

        private void SetPropertyValue(PropertyInfo propertyInfo)
        {
            string _propertyName = propertyInfo.Name;
            object[] attrs = propertyInfo?.GetCustomAttributes(typeof(OptionalAttribute), true);
            bool _isOptional = (attrs?.Length ?? 0) > 0;
            string key = string.IsNullOrWhiteSpace(SectionName) ? _propertyName : $"{SectionName}:{_propertyName}";

            string sValue = _appConfig.GetString(key.ToLower(), !_isOptional);
            object value = null;

            if (propertyInfo.PropertyType.IsArray)
            {
                Array values;

                if (string.IsNullOrEmpty(sValue) && _isOptional)
                {
                    values = Array.CreateInstance(propertyInfo.PropertyType.GetElementType(), 0);
                }
                else
                {
                    string[] arrValues = sValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    values = Array.CreateInstance(propertyInfo.PropertyType.GetElementType(), arrValues.Length);
                    object arrValue;
                    int index = 0;

                    foreach (string arrSValue in arrValues)
                    {
                        if (TryConvertSingleValue(propertyInfo.PropertyType.GetElementType(), arrSValue.Trim(), out arrValue, _isOptional))
                        {
                            values.SetValue(arrValue, index++);
                        }
                        else
                        {
                            throw new ConfigurationParameterValueConversionFailedException(sValue, key, propertyInfo.PropertyType, _propertyName, GetType());
                        }
                    }
                }

                value = values;
            }
            else
            {
                if (!TryConvertSingleValue(propertyInfo.PropertyType, sValue, out value, _isOptional))
                {
                    throw new ConfigurationParameterValueConversionFailedException(sValue, key, propertyInfo.PropertyType, _propertyName, GetType());
                }
            }

            propertyInfo.SetValue(this, value);
        }

        private bool TryConvertSingleValue(Type targetType, string sValue, out object value, bool isOptional)
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
                    if (isOptional)
                    {
                        if (targetType.IsValueType)
                        {
                            value = Activator.CreateInstance(targetType);
                        }
                        else
                        {
                            value = null;
                        }

                        return true;
                    }

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
                    if(isOptional)
                    {
                        if(targetType.IsValueType)
                        {
                            value = Activator.CreateInstance(targetType);
                        }
                        else
                        {
                            value = null;
                        }

                        return true;
                    }

                    return false;
                }
            }

            return true;
        }
    }
}
