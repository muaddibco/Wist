using Google.Protobuf;
using Grpc.Core;
using System;
using System.Net;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.DataModel.Transactional;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Serializers;
using Wist.Client.Common.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Identity;
using Wist.Network.Interfaces;
using Wist.Proto.Model;

namespace Wist.Client.Common.Communication
{
    [RegisterDefaultImplementation(typeof(INetworkSynchronizer), Lifetime = LifetimeManagement.Singleton)]
    public class NetworkSynchronizer : INetworkSynchronizer
    {
        IBlockCreator _blockCreator;
        ISignatureSupportSerializersFactory _signatureSupportSerializersFactory;
        ICommunicationService _communicationService;

        public NetworkSynchronizer(
            IBlockCreator blockCreator,
            ISignatureSupportSerializersFactory signatureSupportSerializersFactory,
            ICommunicationService communicationService)
        {
            _blockCreator = blockCreator;
            _signatureSupportSerializersFactory = signatureSupportSerializersFactory;
            _communicationService = communicationService;
        }

        #region ============ PUBLIC FUNCTIONS =============  

        public DateTime LastSyncTime { get; set; }

        public void SendData(IPAddress address, int port, ChannelCredentials channelCredentials, IKey privateKey, IKey targetKey)
        {
            Channel channel = new Channel(address.Address.ToString(), port, channelCredentials);
            SyncManager.SyncManagerClient syncManagerClient = new SyncManager.SyncManagerClient(channel);
            TransactionalChainManager.TransactionalChainManagerClient transactionalChainManagerClient = new TransactionalChainManager.TransactionalChainManagerClient(channel);

            LastSyncBlock lastSyncBlock = syncManagerClient.GetLastSyncBlock(new Empty());
            TransactionalBlockEssense transactionalBlockEssense = transactionalChainManagerClient.GetLastTransactionalBlock(
                new TransactionalBlockRequest { PublicKey = ByteString.CopyFrom(privateKey.Value) });

            byte[] syncHash = lastSyncBlock.Hash.ToByteArray();
            uint nonce = 1111;
            //byte[] powHash = GetPowHash(syncHash, nonce); // TODO: KIRILL - what is this
            //byte[] targetAddress = GetRandomTargetAddress();// TODO: KIRILL - what is this

            ulong blockHeight = transactionalBlockEssense.Height + 1;

            TransferFundsBlock transferFundsBlock = _blockCreator.GetInstance(BlockTypes.Transaction_TransferFunds) as TransferFundsBlock;
            transferFundsBlock.SyncBlockHeight = lastSyncBlock.Height;
            transferFundsBlock.BlockHeight = blockHeight;
            transferFundsBlock.Nonce = nonce;
            //transferFundsBlock.PowHash = powHash;
            transferFundsBlock.HashPrev = transactionalBlockEssense.Hash.ToByteArray();
            //transferFundsBlock.TargetOriginalHash = targetAddress;
            transferFundsBlock.UptodateFunds = transactionalBlockEssense.UpToDateFunds > 0 ? transactionalBlockEssense.UpToDateFunds - blockHeight : 100000;


            ISignatureSupportSerializer transferFundsSerializer = _signatureSupportSerializersFactory.Create(transferFundsBlock);
            transferFundsSerializer.FillBodyAndRowBytes();

            RegistryRegisterBlock transactionRegisterBlock = _blockCreator.GetInstance(BlockTypes.Registry_Register) as RegistryRegisterBlock;
            transactionRegisterBlock.SyncBlockHeight = lastSyncBlock.Height;
            transactionRegisterBlock.BlockHeight = blockHeight;
            transactionRegisterBlock.Nonce = nonce;
            transactionRegisterBlock.PowHash = powHash;
            transactionRegisterBlock.TransactionHeader = new TransactionHeader
            {
                ReferencedPacketType = PacketType.TransactionalChain,
                ReferencedBlockType = BlockTypes.Transaction_TransferFunds,
                ReferencedHeight = blockHeight,
                ReferencedBodyHash = _hashCalculation.CalculateHash(transferFundsBlock.NonHeaderBytes),
                ReferencedTargetHash = targetAddress
            };

            ISignatureSupportSerializer signatureSupportSerializer = _signatureSupportSerializersFactory.Create(transactionRegisterBlock);

            _communicationService.PostMessage(targetKey, signatureSupportSerializer);
            _communicationService.PostMessage(targetKey, transferFundsSerializer);
        }

        public bool ApproveDataSent()
        {
            return true;
        }

        #endregion

        #region ============ PRIVATE FUNCTIONS ============ 


        #endregion

    }
}
