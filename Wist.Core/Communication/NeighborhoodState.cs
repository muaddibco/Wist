using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.States;

namespace Wist.Core.Communication
{
    [RegisterExtension(typeof(IState), Lifetime = LifetimeManagement.Singleton)]
    /// <summary>
    /// Class that contains general list of neighbor nodes
    /// </summary>
    public class NeighborhoodState : NeighborhoodStateBase
    {
        public const string NAME = nameof(NeighborhoodState);

        public override string Name => NAME;
    }
}
