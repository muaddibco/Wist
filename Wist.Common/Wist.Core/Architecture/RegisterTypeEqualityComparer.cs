using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Wist.Core.Architecture
{
    internal class RegisterTypeEqualityComparer : IEqualityComparer<RegisterType>
    {
        public bool Equals(RegisterType x, RegisterType y)
        {
            return x.Implements == y.Implements;
        }

        public int GetHashCode(RegisterType obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}
