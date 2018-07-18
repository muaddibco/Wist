using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Logging;
using Wist.Node.Core.Interfaces;
using Wist.Node.Core.Roles;

namespace Wist.Simulation.Load
{

    [RegisterExtension(typeof(IModule), Lifetime = LifetimeManagement.Singleton)]
    public class SimpleLoadModule : ModuleBase
    {
        public SimpleLoadModule(ILoggerService loggerService) : base(loggerService)
        {
        }

        public override string Name => nameof(SimpleLoadModule);

        protected override void InitializeInner()
        {
        }
    }
}
