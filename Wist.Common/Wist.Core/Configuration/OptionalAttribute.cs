using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Core.Configuration
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class OptionalAttribute : Attribute
    {
    }
}
