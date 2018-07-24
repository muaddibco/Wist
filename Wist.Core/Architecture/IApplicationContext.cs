using System;
using System.Collections.Generic;
using System.Text;
using Unity;

namespace Wist.Core.Architecture
{
    [ServiceContract]
    public interface IApplicationContext
    {
        UnityContainer Container { get; }
    }
}
