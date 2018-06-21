using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Wist.Communication.Interfaces;
using Wist.Communication.Sockets;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Logging;
using Wist.Node.Core.Interfaces;

namespace Wist.Node.Core.Roles
{
    [RegisterExtension(typeof(IModule), Lifetime = LifetimeManagement.Singleton)]
    public class FullNodeModule : ModuleBase
    {
        public FullNodeModule(ILoggerService loggerService) : base(loggerService)
        {
        }

        public override string Name => nameof(FullNodeModule);

        protected override void InitializeInner()
        {
        }
    }
}
