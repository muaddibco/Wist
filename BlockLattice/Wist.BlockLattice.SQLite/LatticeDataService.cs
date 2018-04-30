using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wist.BlockLattice.DataModel;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Aspects;
using Wist.Core.ExtensionMethods;
using Z.EntityFramework.Plus;

namespace Wist.BlockLattice.MySql
{
    [InitializationMandatory]
    public class LatticeDataService : ISupportInitialization
    {
        private static readonly object _sync = new object();
        private static LatticeDataService _instance = null;

        private readonly DataContext _dataContext;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private bool _isSaving;

        private LatticeDataService()
        {
            _dataContext = new DataContext();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public static LatticeDataService Instance
        {
            get
            {
                if(_instance == null)
                {
                    lock(_sync)
                    {
                        if(_instance == null)
                        {
                            _instance = new LatticeDataService();
                            _instance.Initialize();
                        }
                    }
                }

                return _instance;
            }
        }

        public bool IsInitialized { get; private set; }

        public void CreateGenesisBlock(byte[] originalHash)
        {
            if (originalHash == null)
                throw new ArgumentNullException(nameof(originalHash));

            if (originalHash.Length != 64)
                throw new ArgumentException("Hash value must be 64 bytes length");

            TransactionalGenesis account = new TransactionalGenesis() { OriginalHash = originalHash.ToHexString() };
            
            _dataContext.TransactionalGenesises.AddAsync(account);
        }

        public HashSet<TransactionalBlock> GetLastTransactionalBlocks(byte[] originalHash)
        {
            string hashValue = originalHash.ToHexString();

            var deferredGenesis = _dataContext.TransactionalGenesises.DeferredFirstOrDefault(g => g.OriginalHash == hashValue).FutureValue();

            _dataContext.TransactionalBlocks.Where(b => b.TransactionalGenesis.OriginalHash == hashValue).DeferredMax(b => b.BlockOrder).FirstOrDefault();
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
