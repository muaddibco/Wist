using System.Diagnostics;
using Unity;
using Wist.Core.Architecture.Enums;

namespace Wist.Core.Architecture
{
    [RegisterDefaultImplementation(typeof(IApplicationContext), Lifetime = LifetimeManagement.Singleton)]
    public class ApplicationContext : IApplicationContext
    {
        public ApplicationContext()
        {
            InstanceName = Process.GetCurrentProcess().ProcessName;
        }

        public IUnityContainer Container { get; set; }

        public string InstanceName { get; set; }
    }

}
