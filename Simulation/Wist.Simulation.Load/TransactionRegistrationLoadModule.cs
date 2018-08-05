using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wist.BlockLattice.Core;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Communication.Interfaces;
using Wist.Communication.Sockets;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Configuration;
using Wist.Core.Cryptography;
using Wist.Core.Identity;
using Wist.Core.Logging;
using Wist.Core.PerformanceCounters;
using Wist.Node.Core.Interfaces;
using Wist.Node.Core.Roles;
using Wist.Simulation.Load.PerformanceCounters;

namespace Wist.Simulation.Load
{
    [RegisterExtension(typeof(IModule), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionRegistrationLoadModule : LoadModuleBase
    {
        public TransactionRegistrationLoadModule(ILoggerService loggerService, IClientCommunicationServiceRepository clientCommunicationServiceRepository, IConfigurationService configurationService, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, ISignatureSupportSerializersFactory signatureSupportSerializersFactory, INodesDataService nodesDataService, ICryptoService cryptoService, IPerformanceCountersRepository performanceCountersRepository)
            : base(loggerService, clientCommunicationServiceRepository, configurationService, identityKeyProvidersRegistry, signatureSupportSerializersFactory, nodesDataService, cryptoService, performanceCountersRepository)
        {
        }

        public override string Name => nameof(TransactionRegistrationLoadModule);

        protected override void InitializeInner()
        {
            base.InitializeInner();

            TransactionRegisterBlock transactionRegisterBlock = new TransactionRegisterBlock
            {
                SyncBlockOrder = 0,
                BlockHeight = 1,
                Key = _key,
                Nonce = 1234,
                HashNonce = new byte[Globals.POW_HASH_SIZE],
                POWType = Globals.POW_TYPE,
                ReferencedPacketType = PacketType.TransactionalChain,
                ReferencedBlockType = BlockTypes.Transaction_Confirm,
                ReferencedHeight = 1234,
                ReferencedBodyHash = new byte[Globals.HASH_SIZE]
            };

            ISignatureSupportSerializer signatureSupportSerializer = _signatureSupportSerializersFactory.Create(transactionRegisterBlock);
            _communicationService.PostMessage(_keyTarget, signatureSupportSerializer);

            _loadCountersService.SentMessages.Increment();

        }
    }
}
