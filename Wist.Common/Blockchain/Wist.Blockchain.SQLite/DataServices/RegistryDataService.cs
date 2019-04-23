using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wist.Blockchain.Core;
using Wist.Blockchain.Core.DAL.Keys;
using Wist.Blockchain.Core.DataModel;
using Wist.Blockchain.Core.DataModel.Registry;
using Wist.Blockchain.Core.Enums;
using Wist.Blockchain.Core.Interfaces;
using Wist.Blockchain.DataModel;
using Wist.Blockchain.SQLite.DataAccess;
using Wist.Core;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.ExtensionMethods;
using Wist.Core.HashCalculations;
using Wist.Core.Identity;
using Wist.Core.Models;
using Wist.Core.Translators;

namespace Wist.Blockchain.SQLite.DataServices
{
    [RegisterExtension(typeof(IChainDataService), Lifetime = LifetimeManagement.Singleton)]
    public class RegistryDataService : IChainDataService
    {
        private readonly ITranslatorsRepository _translatorsRepository;
        private readonly IHashCalculation _defaultHashCalculation;

        public RegistryDataService(ITranslatorsRepository translatorsRepository, IHashCalculationsRepository hashCalculationsRepository)
        {
            _translatorsRepository = translatorsRepository;
            _defaultHashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
        }

        public PacketType PacketType => PacketType.Registry;

        public void Add(PacketBase item)
        {
            if(item is RegistryFullBlock block)
            {
                //TODO: shardId must be taken from somewhere
                DataAccessService.Instance.AddRegistryFullBlock(block.SyncBlockHeight, block.BlockHeight, block.StateWitnesses.Length + block.UtxoWitnesses.Length, block.RawData.ToArray(), _defaultHashCalculation.CalculateHash(block.RawData));
            }
        }

        public bool AreServiceActionsAllowed(IKey key)
        {
            throw new NotImplementedException();
        }

        public PacketBase Get(IDataKey key)
        {
            if(key is DoubleHeightKey heightKey)
            {
                TransactionsRegistryBlock transactionsRegistryBlock = DataAccessService.Instance.GetTransactionsRegistryBlock(heightKey.Height1, heightKey.Height2);

                PacketBase blockBase = _translatorsRepository.GetInstance<TransactionsRegistryBlock, PacketBase>().Translate(transactionsRegistryBlock);

                return blockBase;
            }
            else if(key is SyncHashKey syncTransactionKey)
            {
                List<TransactionsRegistryBlock> transactionsRegistryBlocks = DataAccessService.Instance.GetTransactionsRegistryBlocks(syncTransactionKey.SyncBlockHeight);

                TransactionsRegistryBlock transactionsRegistryBlock = transactionsRegistryBlocks.FirstOrDefault(t => syncTransactionKey.Hash.Equals32(t.Hash));

                if(transactionsRegistryBlock == null)
                {
                    transactionsRegistryBlocks = DataAccessService.Instance.GetTransactionsRegistryBlocks(syncTransactionKey.SyncBlockHeight - 1);
                    transactionsRegistryBlock = transactionsRegistryBlocks.FirstOrDefault(t => syncTransactionKey.Hash.Equals32(t.Hash));
                }

                PacketBase blockBase = null;

                if (transactionsRegistryBlock != null)
                {
                    blockBase = _translatorsRepository.GetInstance<TransactionsRegistryBlock, PacketBase>().Translate(transactionsRegistryBlock);
                }

                return blockBase;
            }

            return null;
        }

        public IEnumerable<PacketBase> GetAll()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PacketBase> GetAll(IDataKey key)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PacketBase> GetAllByKey(IKey key)
        {
            throw new NotImplementedException();
        }

        public List<PacketBase> GetAllLastBlocksByType(ushort blockType)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void Update(IDataKey key, PacketBase item)
        {
            throw new NotImplementedException();
        }
    }
}
