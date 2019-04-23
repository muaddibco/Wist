using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Wist.Core.Aspects
{
    public class TypeVerboserManager
    {
        private static Lazy<Dictionary<Type, ITypeVerboser>> _verbosers = new Lazy<Dictionary<Type, ITypeVerboser>>(() => new Dictionary<Type, ITypeVerboser>
        {
            { typeof(ReflectionTypeLoadException), new ReflectionTypeLoadExceptionVerboser() }
        });

        public static string Verbose(object obj)
        {
            if (obj == null)
            {
                return "null";
            }

            Type type = obj.GetType();

            if (_verbosers.Value.ContainsKey(type))
            {
                return _verbosers.Value[type].Verbose(obj);
            }

            return obj.ToString();
        }
    }
}
