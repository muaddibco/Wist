using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Wist.Core.Aspects
{
    public class ReflectionTypeLoadExceptionVerboser : ITypeVerboser
    {
        public string Verbose(object obj)
        {
            if (obj == null)
            {
                return "null";
            }

            var ex = obj as ReflectionTypeLoadException;
            if (ex == null)
            {
                return obj.ToString();
            }

            return ToString(ex);
        }

        private String ToString(ReflectionTypeLoadException ex)
        {
            StringBuilder result = new StringBuilder();

            if (ex.Message == null || ex.Message.Length <= 0)
            {
                result.AppendLine(ex.GetType().FullName);
            }
            else
            {
                result.AppendLine(ex.GetType().FullName + ": " + ex.Message);
            }

            if (ex.InnerException != null)
            {
                result.AppendLine(" ---> " + ex.InnerException);
            }

            if (ex.StackTrace != null)
            {
                result.AppendLine(Environment.NewLine + ex.StackTrace);
            }

            if (ex.LoaderExceptions != null)
            {
                result.AppendFormat("{0} LoaderExceptions\r\n", ex.LoaderExceptions.Length);
                result.AppendLine("----------");

                for (int i = 0; i < ex.LoaderExceptions.Length; i++)
                {
                    result.AppendFormat("Loader Exception {0}: {1}\r\n", i + 1, ex.LoaderExceptions[i]);
                }
            }

            return result.ToString();
        }
    }
}
