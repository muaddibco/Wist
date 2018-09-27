using System;
using System.Linq;
using Wist.BlockLattice.DataModel;

namespace Wist.BlockLattice.SQLite.DataAccess
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

        public void AddSynchronizationRegistryCombinedBlock(ulong blockHeight, byte[] content)
        {
            lock (_sync)
            {
                _dataContext.RegistryCombinedBlocks.Add(new RegistryCombinedBlock
                {
                    RegistryCombinedBlockId = blockHeight,
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
