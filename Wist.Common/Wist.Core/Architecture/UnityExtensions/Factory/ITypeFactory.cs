using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Core.Architecture.UnityExtensions.Factory
{
    public interface ITypeFactory
    {
        object CreateInstance(Type typeToCreate);
    }
}
