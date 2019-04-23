using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using Wist.BlockchainExplorer.Desktop.Models;
using Wist.BlockchainExplorer.Desktop.Services;
using Wist.Blockchain.Core.DataModel;
using Wist.Client.Common.Aspects;
using Wist.Client.Common.Mvvm.ViewModels;
using Wist.Core.Models;

namespace Wist.BlockchainExplorer.Desktop.ViewModels
{
    public class SyncBlockViewModel : ViewModelBase, IObserver<BulkUpdate>
    {
        private readonly Timer _timer;
        private readonly IUpdaterService _updaterService;
        private IDisposable _unsubscriber;
        private bool _initialized = false;
        private RegistryFullBlockInfo _selectedRegistryFullBlockInfo;
        private TransactionHeaderBase _selectedTransactionHeader;

        public SyncBlockViewModel(IUpdaterService updaterService)
        {
            _updaterService = updaterService;
            _unsubscriber = _updaterService.Subscribe(this);

            SyncBlockInfos = new ObservableCollection<SyncBlockInfo>();
            CombinedBlockInfos = new ObservableCollection<CombinedBlockInfo>();
            RegistryFullBlockInfos = new ObservableCollection<RegistryFullBlockInfo>();

            _timer = new Timer(new TimerCallback(o =>
            {
                if (!_initialized)
                {
                    lock (_updaterService)
                    {
                        if (!_initialized)
                        {
                            _updaterService.Initialize();
                            _initialized = true;
                        }
                    }
                }

                _updaterService.Update();
            }), null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }

        [BindableProperty]
        public SyncBlockInfo SelectedSyncBlockInfo { get; set; }

        public ObservableCollection<SyncBlockInfo> SyncBlockInfos { get; set; }

        [BindableProperty]
        public CombinedBlockInfo SelectedCombinedBlockInfo { get; set; }

        public ObservableCollection<CombinedBlockInfo> CombinedBlockInfos { get; set; }

        [BindableProperty]
        public RegistryFullBlockInfo SelectedRegistryFullBlockInfo
        {
            get => _selectedRegistryFullBlockInfo;
            set
            {
                _selectedRegistryFullBlockInfo = value;

                if (_selectedRegistryFullBlockInfo != null)
                {
                    UpdateTransactionHeaders(_selectedRegistryFullBlockInfo.SyncBlockHeight, _selectedRegistryFullBlockInfo.Round);
                    UpdateTransactionInfos(_selectedRegistryFullBlockInfo.SyncBlockHeight, _selectedRegistryFullBlockInfo.Round);
                }
                else
                {
                    TransactionHeaders = null;
                }
            }
        }

        public ObservableCollection<RegistryFullBlockInfo> RegistryFullBlockInfos { get; set; }

        [BindableProperty]
        public List<TransactionHeaderBase> TransactionHeaders { get; set; }

        [BindableProperty]
        public TransactionHeaderBase SelectedTransactionHeader
        {
            get => _selectedTransactionHeader;
            set
            {
                _selectedTransactionHeader = value;
            }
        }

        [BindableProperty]
        public List<PacketBase> Transactions { get; set; }

        public void OnCompleted() => throw new NotImplementedException();
        public void OnError(Exception error) => throw new NotImplementedException();

        public void OnNext(BulkUpdate bulkUpdate)
        {
            if (bulkUpdate != null)
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    if (bulkUpdate.SyncBlockInfos != null)
                    {
                        foreach (var syncBlockInfo in bulkUpdate.SyncBlockInfos)
                        {
                            SyncBlockInfos.Add(syncBlockInfo);
                        }
                    }

                    if (bulkUpdate.CombinedBlockInfos != null)
                    {
                        foreach (var combinedBlockInfo in bulkUpdate.CombinedBlockInfos)
                        {
                            CombinedBlockInfos.Add(combinedBlockInfo);

                            if (combinedBlockInfo.RegistryFullBlockInfos != null)
                            {
                                foreach (RegistryFullBlockInfo registryFullBlockInfo in combinedBlockInfo.RegistryFullBlockInfos)
                                {
                                    RegistryFullBlockInfos.Add(registryFullBlockInfo);
                                }
                            }
                        }
                    }
                });
            }
        }

        private async void UpdateTransactionHeaders(ulong syncBlockHeight, ulong round)
        {
            List<TransactionHeaderBase> transactionInfos = await _updaterService.GetTransactionHeadersInfo(syncBlockHeight, round).ConfigureAwait(false);

            App.Current.Dispatcher.Invoke(() => { TransactionHeaders = transactionInfos; });
        }

        private async void UpdateTransactionInfos(ulong syncBlockHeight, ulong round)
        {
            List<PacketBase> transactions = await _updaterService.GetTransactions(syncBlockHeight, round);

            App.Current.Dispatcher.Invoke(() => { Transactions = transactions; });
        }
    }
}
