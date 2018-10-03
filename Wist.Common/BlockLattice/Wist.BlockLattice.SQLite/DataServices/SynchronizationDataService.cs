using System;
using System.Collections.Generic;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Synchronization;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.BlockLattice.DataModel;
using Wist.BlockLattice.SQLite.DataAccess;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Identity;
using Wist.Core.Translators;

namespace Wist.BlockLattice.SQLite.DataServices
{
    [RegisterExtension(typeof(IChainDataService), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationDataService : IChainDataService
    {
        private readonly ITranslatorsRepository _mapperFactory;

        public SynchronizationDataService(ITranslatorsRepository mapperFactory)
        {
            _mapperFactory = mapperFactory;
        }

        public PacketType ChainType => PacketType.Synchronization;

        public void Add(BlockBase item)
        {
            if (item is SynchronizationConfirmedBlock synchronizationConfirmedBlock)
            {
                DataAccessService.Instance.AddSynchronizationBlock(synchronizationConfirmedBlock.BlockHeight, DateTime.Now, synchronizationConfirmedBlock.ReportedTime, synchronizationConfirmedBlock.NonHeaderBytes.ToArray());
            }

            if(item is SynchronizationRegistryCombinedBlock combinedBlock)
            {
                DataAccessService.Instance.AddSynchronizationRegistryCombinedBlock(combinedBlock.BlockHeight, combinedBlock.SyncBlockHeight, combinedBlock.BlockHeight, combinedBlock.NonHeaderBytes.ToArray());
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
            switch (blockType)
            {
                case BlockTypes.Synchronization_RegistryCombinationBlock:
                    {
                        RegistryCombinedBlock block = DataAccessService.Instance.GetLastRegistryCombinedBlock();
                        return new List<BlockBase> { _mapperFactory.GetInstance<RegistryCombinedBlock, BlockBase>().Translate(block) };
                    }
                case BlockTypes.Synchronization_ConfirmedBlock:
                    {
                        SynchronizationBlock block = DataAccessService.Instance.GetLastSynchronizationBlock();
                        return  new List<BlockBase> { _mapperFactory.GetInstance<SynchronizationBlock, BlockBase>().Translate(block) };
                    }
                default:
                    return null;
            }
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
