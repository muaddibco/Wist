using System;
using System.Collections.Generic;
using System.Text;
using Unity;

namespace Wist.Core.Architecture
{
    [ServiceContract]
    public interface IApplicationContext
    {
        IUnityContainer Container { get; set; }

        string InstanceName { get; set; }
    }
}
