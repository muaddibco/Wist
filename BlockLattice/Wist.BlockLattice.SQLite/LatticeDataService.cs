using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Interfaces;
using Wist.BlockLattice.MySql.DataModel;
using Wist.Core;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Aspects;

namespace Wist.BlockLattice.MySql
{
    [RegisterDefaultImplementation(typeof(ILatticeDataService), Lifetime = LifetimeManagement.Singleton)]
    [InitializationMandatory]
    public class LatticeDataService : ILatticeDataService, ISupportInitialization
    {
        private readonly DataContext _dataContext;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private bool _isSaving;

        public LatticeDataService()
        {
            _dataContext = new DataContext();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public bool IsInitialized { get; private set; }

        public void CreateGenesisBlock(byte[] originalHash)
        {
            if (originalHash == null)
                throw new ArgumentNullException(nameof(originalHash));

            if (originalHash.Length != 64)
                throw new ArgumentException("Hash value must be 64 bytes length");

            TransactionalGenesis account = new TransactionalGenesis() { OriginalHash = originalHash };
            
            _dataContext.TransactionalAccounts.AddAsync(account);
        }

        public HashSet<TransactionalBlockBase> GetLastTransactionalBlocks(byte[] originalHash)
        {
            throw new NotImplementedException();
        }

        public TransactionalBlockchain GetTransactionalBlockchain(byte[] originalHash)
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {
            if (IsInitialized)
                return;

            PeriodicTaskFactory.Start(() => 
            {
                if (_isSaving)
                    return;

                _isSaving = true;

                try
                {
                    if (_dataContext.ChangeTracker.HasChanges())
                        _dataContext.SaveChanges();
                }
                finally
                {
                    _isSaving = false;
                }
            }, 500, cancelToken: _cancellationTokenSource.Token);

            IsInitialized = true;
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
