using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Communication.Interfaces;
using Wist.Core.States;

namespace Wist.Node.Core.Registry
{
    public class TransactionsRegistryHandler : IBlocksHandler
    {
        public const string NAME = "TransactionsRegistry";

        private readonly BlockingCollection<TransactionRegisterBlock> _registrationBlocks;
        private readonly IServerCommunicationServicesRegistry _communicationServicesRegistry;
        private IServerCommunicationService _communicationService;

        public TransactionsRegistryHandler(IStatesRepository statesRepository, IServerCommunicationServicesRegistry communicationServicesRegistry)
        {
            _communicationServicesRegistry = communicationServicesRegistry;
        }

        public string Name => NAME;

        public PacketType PacketType => PacketType.Registry;

        public void Initialize(CancellationToken ct)
        {
            _communicationService = _communicationServicesRegistry.GetInstance("GenericUdp");

            Task.Factory.StartNew(() => {
                ProcessBlocks(ct);
            }, ct, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

        public void ProcessBlock(BlockBase blockBase)
        {
            
        }

        #region Private Functions

        private void ProcessBlocks(CancellationToken ct)
        {
            foreach (TransactionRegisterBlock transactionRegisterBlock in _registrationBlocks.GetConsumingEnumerable(ct))
            {

            }
        }

        #endregion PrivateFunctions
    }
}
