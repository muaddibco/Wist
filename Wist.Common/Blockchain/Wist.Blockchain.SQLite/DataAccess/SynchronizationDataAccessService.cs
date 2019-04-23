using System;
using System.Collections.Generic;
using System.Linq;
using Wist.Blockchain.DataModel;

namespace Wist.Blockchain.SQLite.DataAccess
{
    public partial class DataAccessService
    {

        #region Synchronization

        public void AddSynchronizationBlock(ulong blockHeight, DateTime receiveTime, DateTime medianTime, byte[] content)
        {
            lock (_sync)
            {
                _dataContext.SynchronizationBlocks.Add(new SynchronizationBlock
                {
                    SynchronizationBlockId = blockHeight,
                    ReceiveTime = receiveTime,
                    MedianTime = medianTime,
                    BlockContent = content
                });
            }
        }

        public SynchronizationBlock GetLastSynchronizationBlock()
        {
            lock (_sync)
            {
                return _dataContext.SynchronizationBlocks.OrderByDescending(b => b.SynchronizationBlockId).FirstOrDefault();
            }
        }

        public IEnumerable<SynchronizationBlock> GetAllLastSynchronizationBlocks(ulong height)
        {
            lock (_sync)
            {
                //TODO: change back to queriable
                List<SynchronizationBlock> lastSyncBlocks = _dataContext.SynchronizationBlocks.OrderByDescending(b => b.SynchronizationBlockId).Where(b => b.SynchronizationBlockId > height).ToList();
                return lastSyncBlocks;
            }
        }

        public IEnumerable<SynchronizationBlock> GetAllSynchronizationBlocks()
        {
            lock (_sync)
            {
                return _dataContext.SynchronizationBlocks.ToList();
            }
        }

        public IEnumerable<RegistryCombinedBlock> GetAllRegistryCombinedBlocks()
        {
            lock (_sync)
            {
                return _dataContext.RegistryCombinedBlocks.ToList();
            }
        }

        public IEnumerable<RegistryCombinedBlock> GetAllLastRegistryCombinedBlocks(ulong height)
        {
            lock (_sync)
            {
                return _dataContext.RegistryCombinedBlocks.OrderByDescending(b => b.Round).Where(b => b.Round > height).ToList();
            }
        }

        public void AddSynchronizationRegistryCombinedBlock(ulong blockHeight, ulong syncBlockHeight, ulong round, byte[] content)
        {
            lock (_sync)
            {
                _dataContext.RegistryCombinedBlocks.Add(new RegistryCombinedBlock
                {
                    RegistryCombinedBlockId = blockHeight,
                    SyncBlockHeight = syncBlockHeight,
                    Round = round,
                    Content = content
                });
            }
        }

        public RegistryCombinedBlock GetLastRegistryCombinedBlock()
        {
            lock (_sync)
            {
                return _dataContext.RegistryCombinedBlocks.OrderByDescending(b => b.RegistryCombinedBlockId).FirstOrDefault();
            }
        }

        #endregion Synchronization
    }
}
