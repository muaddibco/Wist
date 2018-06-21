using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Communication.Interfaces;
using Wist.Communication.Sockets;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Logging;
using Wist.Node.Core.Interfaces;

namespace Wist.Node.Core.Roles
{
    [RegisterExtension(typeof(IModule), Lifetime = LifetimeManagement.Singleton)]
    public class MasterNodeModule : ModuleBase
    {
        private readonly IBlocksHandlersFactory _blocksProcessorFactory;

        public MasterNodeModule(ILoggerService loggerService, IBlocksHandlersFactory blocksProcessorFactory) : base(loggerService)
        {
            _blocksProcessorFactory = blocksProcessorFactory;
        }

        public override string Name => nameof(MasterNodeModule);

        protected override void InitializeInner()
        {
        }
    }
}
