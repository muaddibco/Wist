using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wist.BlockchainExplorer.Desktop.Models;
using Wist.Blockchain.Core.Enums;
using Wist.Core.ExtensionMethods;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Proto.Model;
using CommonServiceLocator;
using Wist.Core.HashCalculations;
using Wist.Blockchain.Core;
using Wist.Blockchain.Core.DataModel;
using Wist.Blockchain.Core.Parsers;
using System.Numerics;
using Wist.Core.Models;

namespace Wist.BlockchainExplorer.Desktop.Services
{
    [RegisterDefaultImplementation(typeof(IUpdaterService), Lifetime = LifetimeManagement.Singleton)]
    public class UpdaterService : IUpdaterService
    {
        private readonly List<IObserver<BulkUpdate>> _observers;
        private SyncManager.SyncManagerClient _syncManagerClient;
        private TransactionalChainManager.TransactionalChainManagerClient _transactionalChainManager;
        private ulong _lastUpdatedSyncHeight;
        private ulong _lastUpdatedCombinedBlockHeight;
        private readonly object _sync = new object();
        private readonly IBlockParsersRepositoriesRepository _blockParsersRepositoriesRepository;
        private bool _isUpdating;

        public UpdaterService(IBlockParsersRepositoriesRepository blockParsersRepositoriesRepository)
        {
            _observers = new List<IObserver<BulkUpdate>>();
            _blockParsersRepositoriesRepository = blockParsersRepositoriesRepository;
        }

        public void Initialize()
        {
            //byte[] hash = ServiceLocator.Current.GetInstance<IHashCalculationsRepository>().Create(Globals.DEFAULT_HASH).CalculateHash("ffff440000000000000000000000ff56614220131d2a562741432675d1cc32c9aad819f1eca40100ffff4500000000000000085c384c05bbf1b93754c5903014f9b9519f753f773b156ed7611dc5067bb82f00000000000000000100002c1130afeeab4855bc0224e5a7a4fd719d74407443b578789cc87f93a2ac350d093a5618c9e82dec81e38bb019e720295490b1d92f0870edc3b6516fe0bd99015f109903b082d45d58e4af08e637d17cac12b7f592367f4f2dc1378173f4d6f7".HexStringToByteArray());
            //byte[] pow = ServiceLocator.Current.GetInstance<IHashCalculationsRepository>().Create(Globals.POW_TYPE).CalculateHash(hash);
            //BigInteger big = new BigInteger("05BED0C6BFC7ACAD4202A26EDFC83843B7745AE5CB8FA038B6815C7D5EFFBBFF".HexStringToByteArray());
            //big = 0 - big;
            //uint nonce = 0;
            //big += nonce;
            //byte[] bigBytes = big.ToByteArray();
            Channel syncChannel = new Channel("127.0.0.1", 5050, ChannelCredentials.Insecure);
            Channel transactionalChannel = new Channel("127.0.0.1", 5050, ChannelCredentials.Insecure);
            _syncManagerClient = new SyncManager.SyncManagerClient(syncChannel);
            _transactionalChainManager = new TransactionalChainManager.TransactionalChainManagerClient(transactionalChannel);
        }

        public IDisposable Subscribe(IObserver<BulkUpdate> observer)
        {
            lock(_observers)
            {
                _observers.Add(observer);
            }

            return new Subscription(this, observer);
        }

        public async void Update()
        {
            if (_isUpdating)
            {
                return;
            }

            lock (_sync)
            {
                if (_isUpdating)
                {
                    return;
                }

                _isUpdating = true;
            }

            BulkUpdate bulkUpdate = new BulkUpdate();

            try
            {
                ulong lastUpdatedSyncHeight = 0;

                AsyncServerStreamingCall<SyncBlockDescriptor> serverStreamingCall = _syncManagerClient.GetDeltaSyncBlocks(new ByHeightRequest { Height = _lastUpdatedSyncHeight });
                while (await serverStreamingCall.ResponseStream.MoveNext())
                {
                    SyncBlockDescriptor syncBlockDescriptor = serverStreamingCall.ResponseStream.Current;
                    if (syncBlockDescriptor.Height > _lastUpdatedSyncHeight)
                    {
                        bulkUpdate.SyncBlockInfos.Add(new Models.SyncBlockInfo { SyncBlockHeight = syncBlockDescriptor.Height });
                        if (lastUpdatedSyncHeight < syncBlockDescriptor.Height)
                        {
                            lastUpdatedSyncHeight = syncBlockDescriptor.Height;
                        }
                    }
                }

                if (_lastUpdatedSyncHeight < lastUpdatedSyncHeight)
                {
                    _lastUpdatedSyncHeight = lastUpdatedSyncHeight;
                }

                ulong lastUpdatedCombinedBlockHeight = 0;
                AsyncServerStreamingCall<CombinedRegistryBlockInfo> serverStreamingCall2 = _syncManagerClient.GetCombinedRegistryBlocksInfoSinceHeight(new ByHeightRequest { Height = _lastUpdatedCombinedBlockHeight });
                while (await serverStreamingCall2.ResponseStream.MoveNext())
                {
                    CombinedRegistryBlockInfo combinedRegistryBlockInfo = serverStreamingCall2.ResponseStream.Current;

                    if (combinedRegistryBlockInfo.Height > _lastUpdatedCombinedBlockHeight)
                    {
                        bulkUpdate.CombinedBlockInfos.Add(
                            new CombinedBlockInfo
                            {
                                SyncBlockHeight = combinedRegistryBlockInfo.SyncBlockHeight,
                                BlockHeight = combinedRegistryBlockInfo.Height,
                                CombinedRegistryBlocksCount = combinedRegistryBlockInfo.CombinedRegistryBlocksCount,
                                RegistryFullBlockInfos = combinedRegistryBlockInfo.BlockDescriptors.Select(
                                    b => new RegistryFullBlockInfo
                                    {
                                        SyncBlockHeight = b.SyncBlockHeight,
                                        Round = b.Round,
                                        TransactionsCount = b.TransactionsCount
                                    }).ToList()
                            });

                        if (lastUpdatedCombinedBlockHeight < combinedRegistryBlockInfo.Height)
                        {
                            lastUpdatedCombinedBlockHeight = combinedRegistryBlockInfo.Height;
                        }
                    }
                }

                if (_lastUpdatedCombinedBlockHeight < lastUpdatedCombinedBlockHeight)
                {
                    _lastUpdatedCombinedBlockHeight = lastUpdatedCombinedBlockHeight;
                }
            }
            finally
            {
                _isUpdating = false;
            }

            foreach (IObserver<BulkUpdate> observer in _observers)
            {
                observer.OnNext(bulkUpdate);
            }
        }

        public async Task<List<TransactionHeaderBase>> GetTransactionHeadersInfo(ulong syncBlockHeight, ulong round)
        {
            List<TransactionHeaderBase> transactionInfos = new List<TransactionHeaderBase>();

            int order = 0;
            AsyncServerStreamingCall<TransactionRegistryBlockInfo> asyncServerStreaming = _syncManagerClient.GetTransactionRegistryBlockInfos(new FullBlockRequest { SyncBlockHeight = syncBlockHeight, Round = round });
            while(await asyncServerStreaming.ResponseStream.MoveNext().ConfigureAwait(false))
            {
                TransactionRegistryBlockInfo item = asyncServerStreaming.ResponseStream.Current;

                switch (item.HeaderCase)
                {
                    case TransactionRegistryBlockInfo.HeaderOneofCase.AccountedHeader:
                        transactionInfos.Add(new AccountedTransactionHeaderInfo
                        {
                            OrderInBlock = order++,
                            SyncBlockHeight = item.AccountedHeader.SyncBlockHeight,
                            BlockHeight = item.AccountedHeader.ReferencedHeight,
                            PacketType = (PacketType)item.AccountedHeader.ReferencedPacketType,
                            BlockType = (ushort)item.AccountedHeader.ReferencedBlockType,
                            Target = item.AccountedHeader.ReferencedTarget.ToByteArray().ToHexString()
                        });
                        break;
                    case TransactionRegistryBlockInfo.HeaderOneofCase.UtxoHeader:
                        transactionInfos.Add(new UtxoTransactionHeaderInfo
                        {
                            OrderInBlock = order++,
                            SyncBlockHeight = item.UtxoHeader.SyncBlockHeight,
                            PacketType = (PacketType)item.UtxoHeader.ReferencedPacketType,
                            BlockType = (ushort)item.UtxoHeader.ReferencedBlockType,
                            TransactionKey = item.UtxoHeader.ReferencedTransactionKey.ToByteArray().ToHexString(),
                            KeyImage = item.UtxoHeader.KeyImage.ToByteArray().ToHexString(),
                            Target = item.UtxoHeader.ReferencedTarget.ToByteArray().ToHexString()
                        });
                        break;
                }
            }

            return transactionInfos;
        }

        public async Task<List<PacketBase>> GetTransactions(ulong syncBlockHeight, ulong round)
        {
            List<PacketBase> transactionInfos = new List<PacketBase>();

            AsyncServerStreamingCall<TransactionInfo> asyncServerStreaming = _transactionalChainManager.GetTransactionInfos(new FullBlockRequest { SyncBlockHeight = syncBlockHeight, Round = round });
            while (await asyncServerStreaming.ResponseStream.MoveNext().ConfigureAwait(false))
            {
                TransactionInfo item = asyncServerStreaming.ResponseStream.Current;
                IBlockParsersRepository blockParsersRepository = _blockParsersRepositoriesRepository.GetBlockParsersRepository((PacketType)item.PacketType);
                IBlockParser blockParser = blockParsersRepository.GetInstance((ushort)item.BlockType);
                PacketBase blockBase = blockParser.Parse(item.Content.ToByteArray());
                transactionInfos.Add(blockBase);
            }

            return transactionInfos;
        }

        private sealed class Subscription : IDisposable
        {
            private readonly UpdaterService _updaterService;
            private IObserver<BulkUpdate> _observer;

            public Subscription(UpdaterService updaterService, IObserver<BulkUpdate> observer)
            {
                _updaterService = updaterService;
                _observer = observer;
            }

            public void Dispose()
            {
                IObserver<BulkUpdate> observer = _observer;
                if (null != observer)
                {
                    lock (_updaterService._observers)
                    {
                        _updaterService._observers.Remove(observer);
                    }
                    _observer = null;
                }
            }
        }
    }
}
