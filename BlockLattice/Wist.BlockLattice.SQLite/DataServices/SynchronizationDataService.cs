using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Synchronization;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Identity;

namespace Wist.BlockLattice.SQLite.DataServices
{
    [RegisterExtension(typeof(IChainDataService), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationDataService : IChainDataService
    {
        public PacketType ChainType => PacketType.Synchronization;

        public void Add(BlockBase item)
        {
            SynchronizationConfirmedBlock synchronizationConfirmedBlock = item as SynchronizationConfirmedBlock;

            if(synchronizationConfirmedBlock != null)
            {
                LatticeDataService.Instance.AddSynchronizationBlock(synchronizationConfirmedBlock.BlockHeight, DateTime.Now, synchronizationConfirmedBlock.ReportedTime, synchronizationConfirmedBlock.BodyBytes);
            }
        }

        public void CreateGenesisBlock(GenesisBlockBase genesisBlock)
        {
            throw new NotImplementedException();
        }

        public bool DoesChainExist(IKey key)
        {
            throw new NotImplementedException();
        }

        public BlockBase Get(long key)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<BlockBase> GetAll()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<BlockBase> GetAllByKey(IKey key)
        {
            throw new NotImplementedException();
        }

        public List<BlockBase> GetAllLastBlocksByType(ushort blockType)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetAllLastBlocksByType<T>() where T : BlockBase
        {
            throw new NotImplementedException();
        }

        public BlockBase GetBlockByOrder(IKey key, uint order)
        {
            throw new NotImplementedException();
        }

        public GenesisBlockBase GetGenesisBlock(IKey key)
        {
            throw new NotImplementedException();
        }

        public BlockBase GetLastBlock(IKey key)
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {
        }

        public void Update(long key, BlockBase item)
        {
            throw new NotImplementedException();
        }
    }
}
