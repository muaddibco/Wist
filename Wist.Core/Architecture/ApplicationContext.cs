using Unity;
using Wist.Core.Architecture.Enums;

namespace Wist.Core.Architecture
{
    [RegisterDefaultImplementation(typeof(IApplicationContext), Lifetime = LifetimeManagement.Singleton)]
    public class ApplicationContext : IApplicationContext
    {
        public ApplicationContext()
        {
        }

        public UnityContainer Container { get; set; }
    }

}
