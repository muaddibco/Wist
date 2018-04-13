using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.ExtensionMethods;

namespace Wist.Core.Architecture
{
    /// <summary>
    /// Attribute decorating classes or interfaces and designating definition for services
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false)]
    public class ServiceContract : Attribute
    {
        public Type Contract { get; set; }

        public override string ToString()
        {
            return $"Service Contract - {Contract.FullNameWithAssemblyPath()}";
        }
    }
}
