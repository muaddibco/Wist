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
using Wist.Node.Core.Synchronization;

namespace Wist.Node.Core.Roles
{
    [RegisterExtension(typeof(IModule), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationProducerModule : ModuleBase
    {
        private readonly IRole _syncMasterRole;
        private readonly IRolesRegistry _rolesRegistry;

        public SynchronizationProducerModule(ILoggerService loggerService, IRolesRepository rolesRepository, IRolesRegistry rolesRegistry) : base(loggerService)
        {
            _syncMasterRole = rolesRepository.GetInstance(nameof(SyncMasterRole));
            _rolesRegistry = rolesRegistry;
        }

        public override string Name => nameof(SynchronizationProducerModule);

        protected override void InitializeInner()
        {
            _rolesRegistry.RegisterInstance(_syncMasterRole);
        }
    }
}
