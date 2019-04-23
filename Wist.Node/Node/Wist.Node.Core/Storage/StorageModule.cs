using System;
using System.Collections.Generic;
using System.Text;
using Wist.Blockchain.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Logging;
using Wist.Core.Modularity;
using Wist.Node.Core.Common;

namespace Wist.Node.Core.Storage
{
    [RegisterExtension(typeof(IModule), Lifetime = LifetimeManagement.Singleton)]
    public class StorageModule : ModuleBase
    {
        public const string NAME = nameof(StorageModule);
        private readonly IBlocksHandlersRegistry _blocksHandlersFactory;

        public StorageModule(ILoggerService loggerService, IBlocksHandlersRegistry blocksHandlersFactory) : base(loggerService)
        {
            _blocksHandlersFactory = blocksHandlersFactory;
        }

        public override string Name => NAME;

        public override void Start()
        {
            
        }

        protected override void InitializeInner()
        {
            IBlocksHandler transactionalStorageBlocksHandler = _blocksHandlersFactory.GetInstance(TransactionalStorageHandler.NAME);
            _blocksHandlersFactory.RegisterInstance(transactionalStorageBlocksHandler);
            transactionalStorageBlocksHandler.Initialize(_cancellationToken);
            IBlocksHandler utxoConfidentialStorageBlocksHandler = _blocksHandlersFactory.GetInstance(UtxoConfidentialStorageHandler.NAME);
            _blocksHandlersFactory.RegisterInstance(utxoConfidentialStorageBlocksHandler);
            utxoConfidentialStorageBlocksHandler.Initialize(_cancellationToken);
        }
    }
}
