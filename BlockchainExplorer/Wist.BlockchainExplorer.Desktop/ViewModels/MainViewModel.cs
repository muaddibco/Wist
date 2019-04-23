using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wist.BlockchainExplorer.Desktop.Services;
using Wist.Client.Common.Mvvm.ViewModels;

namespace Wist.BlockchainExplorer.Desktop.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public SyncBlockViewModel SyncBlockViewModel { get; set; }

        public MainViewModel(IUpdaterService updaterService)
        {
            SyncBlockViewModel = new SyncBlockViewModel(updaterService);
        }
    }
}
