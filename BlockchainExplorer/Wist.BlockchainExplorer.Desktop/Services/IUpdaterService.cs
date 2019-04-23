using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wist.BlockchainExplorer.Desktop.Models;
using Wist.Blockchain.Core.DataModel;
using Wist.Core.Architecture;
using Wist.Core.Models;

namespace Wist.BlockchainExplorer.Desktop.Services
{
    [ServiceContract]
    public interface IUpdaterService : IObservable<BulkUpdate>
    {
        void Initialize();

        void Update();

        Task<List<TransactionHeaderBase>> GetTransactionHeadersInfo(ulong syncBlockHeight, ulong round);

        Task<List<PacketBase>> GetTransactions(ulong syncBlockHeight, ulong round);
    }
}
