using System;
using System.Collections.Generic;
using System.Linq;
using Wist.Blockchain.Core.DAL.Keys;
using Wist.Blockchain.Core.DataModel;
using Wist.Blockchain.Core.DataModel.Synchronization;
using Wist.Blockchain.Core.Enums;
using Wist.Blockchain.Core.Interfaces;
using Wist.Blockchain.DataModel;
using Wist.Blockchain.SQLite.DataAccess;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Identity;
using Wist.Core.Models;
using Wist.Core.Translators;

namespace Wist.Blockchain.SQLite.DataServices
{
    [RegisterExtension(typeof(IChainDataService), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationDataService : IChainDataService
    {
        private readonly ITranslatorsRepository _translatorsRepository;

        public SynchronizationDataService(ITranslatorsRepository translatorsRepository)
        {
            _translatorsRepository = translatorsRepository;
        }

        public PacketType PacketType => PacketType.Synchronization;

        public void Add(PacketBase item)
        {
            if (item is SynchronizationConfirmedBlock synchronizationConfirmedBlock)
            {
                DataAccessService.Instance.AddSynchronizationBlock(synchronizationConfirmedBlock.BlockHeight, DateTime.Now, synchronizationConfirmedBlock.ReportedTime, synchronizationConfirmedBlock.RawData.ToArray());
            }

            if(item is SynchronizationRegistryCombinedBlock combinedBlock)
            {
                DataAccessService.Instance.AddSynchronizationRegistryCombinedBlock(combinedBlock.BlockHeight, combinedBlock.SyncBlockHeight, combinedBlock.BlockHeight, combinedBlock.RawData.ToArray());
            }
        }

        public bool AreServiceActionsAllowed(IKey key)
        {
            return true;
        }

        public PacketBase Get(IDataKey key)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PacketBase> GetAll()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PacketBase> GetAll(IDataKey key)
        {
            if(key is BlockTypeLowHeightKey blockTypeLowHeightKey)
            {

                if (blockTypeLowHeightKey.BlockType == BlockTypes.Synchronization_ConfirmedBlock)
                {
                    return DataAccessService.Instance.GetAllLastSynchronizationBlocks(blockTypeLowHeightKey.Height).Select(b => _translatorsRepository.GetInstance<SynchronizationBlock, PacketBase>().Translate(b));
                }
                else if(blockTypeLowHeightKey.BlockType == BlockTypes.Synchronization_RegistryCombinationBlock)
                {
                    return DataAccessService.Instance.GetAllLastRegistryCombinedBlocks(blockTypeLowHeightKey.Height).OrderBy(b => b.RegistryCombinedBlockId).Select(b => _translatorsRepository.GetInstance<RegistryCombinedBlock, PacketBase>().Translate(b));
                }
            }
            else if(key is BlockTypeKey blockTypeKey)
            {
                if (blockTypeKey.BlockType == BlockTypes.Synchronization_ConfirmedBlock)
                {
                    return DataAccessService.Instance.GetAllSynchronizationBlocks().Select(b => _translatorsRepository.GetInstance<SynchronizationBlock, PacketBase>().Translate(b));
                }
                else if (blockTypeKey.BlockType == BlockTypes.Synchronization_RegistryCombinationBlock)
                {
                    return DataAccessService.Instance.GetAllRegistryCombinedBlocks().Select(b => _translatorsRepository.GetInstance<RegistryCombinedBlock, PacketBase>().Translate(b));
                }
            }

            return null;
        }

        public IEnumerable<PacketBase> GetAllByKey(IKey key)
        {
            throw new NotImplementedException();
        }

        public List<PacketBase> GetAllLastBlocksByType(ushort blockType)
        {
            switch (blockType)
            {
                case BlockTypes.Synchronization_RegistryCombinationBlock:
                    {
                        RegistryCombinedBlock block = DataAccessService.Instance.GetLastRegistryCombinedBlock();
                        return new List<PacketBase> { _translatorsRepository.GetInstance<RegistryCombinedBlock, PacketBase>().Translate(block) };
                    }
                case BlockTypes.Synchronization_ConfirmedBlock:
                    {
                        SynchronizationBlock block = DataAccessService.Instance.GetLastSynchronizationBlock();
                        return  new List<PacketBase> { _translatorsRepository.GetInstance<SynchronizationBlock, PacketBase>().Translate(block) };
                    }
                default:
                    return null;
            }
        }

        public IEnumerable<T> GetAllLastBlocksByType<T>() where T : PacketBase
        {
            throw new NotImplementedException();
        }

        public PacketBase GetBlockByOrder(IKey key, uint order)
        {
            throw new NotImplementedException();
        }

        public PacketBase GetLastBlock(IKey key)
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {
        }

        public void Update(IDataKey key, PacketBase item)
        {
            throw new NotImplementedException();
        }
    }
}
