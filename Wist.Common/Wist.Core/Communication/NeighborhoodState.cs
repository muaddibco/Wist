using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.States;

namespace Wist.Core.Communication
{
    [RegisterExtension(typeof(IState), Lifetime = LifetimeManagement.Singleton)]
    /// <summary>
    /// Class that contains general list of neighbor nodes. This class is used by participants at all roles in order have random neighbors of the same role
    /// </summary>
    public class NeighborhoodState : NeighborhoodStateBase
    {
        public const string NAME = nameof(INeighborhoodState);

        public override string Name => NAME;
    }
}
