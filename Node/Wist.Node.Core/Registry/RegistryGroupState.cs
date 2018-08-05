using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.States;

namespace Wist.Node.Core.Registry
{
    [RegisterExtension(typeof(IState), Lifetime = LifetimeManagement.Singleton)]
    public class RegistryGroupState : NeighborhoodStateBase
    {
        public const string NAME = nameof(RegistryGroupState);

        public override string Name => NAME;
    }
}
