using System;
using System.Collections.Generic;
using Wist.Blockchain.Core.Enums;
using Wist.Blockchain.Core.DataModel.Transactional;
using Wist.Blockchain.Core.Interfaces;
using Wist.Blockchain.DataModel;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Blockchain.Core.DataModel;
using Wist.Core.Translators;
using Wist.Core.Identity;
using Wist.Blockchain.Core.Serializers;
using Wist.Blockchain.SQLite.DataAccess;
using Wist.Blockchain.Core.DAL.Keys;
using Wist.Core.Models;

namespace Wist.Blockchain.SQLite.DataServices
{
    [RegisterExtension(typeof(IChainDataService), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionalChainDataService : IChainDataService
    {
        private readonly ITranslatorsRepository _mapperFactory;

        public PacketType PacketType => PacketType.Transactional;


        public TransactionalChainDataService(ITranslatorsRepository mapperFactory)
        {
            _mapperFactory = mapperFactory;
        }

        public void Add(PacketBase block)
        {
            if(block is TransactionalPacketBase transactionalBlockBase)
            {
                IKey key = transactionalBlockBase.Signer;

                DataAccessService.Instance.AddTransactionalBlock(key, transactionalBlockBase.SyncBlockHeight, transactionalBlockBase.BlockType, transactionalBlockBase.BlockHeight, transactionalBlockBase.RawData.ToArray());
            }
        }

        public bool AreServiceActionsAllowed(IKey key)
        {
            return DataAccessService.Instance.IsTransactionalIdentityExist(key);
        }

        public PacketBase[] GetAllBlocks(IKey key)
        {
            throw new NotImplementedException();
        }

        public PacketBase GetBlockByOrder(IKey key, uint order)
        {
            throw new NotImplementedException();
        }

        public PacketBase GetLastBlock(IKey key)
        {
            TransactionalBlock transactionalBlock = DataAccessService.Instance.GetLastTransactionalBlock(key);

            if (transactionalBlock != null)
            {
                ITranslator<TransactionalBlock, PacketBase> mapper = _mapperFactory.GetInstance<TransactionalBlock, PacketBase>();

                PacketBase block = mapper?.Translate(transactionalBlock);

                return block;
            }

            return null;
        }

        public IEnumerable<T> GetAllLastBlocksByType<T>() where T : PacketBase
        {
            List<T> blocks = new List<T>();

            if(typeof(T) == typeof(TransactionalPacketBase))
            {
                foreach (TransactionalIdentity transactionalIdentity in DataAccessService.Instance.GetAllTransctionalIdentities())
                {
                    TransactionalBlock transactionalBlock = DataAccessService.Instance.GetLastTransactionalBlock(transactionalIdentity);

                    ITranslator<TransactionalBlock, PacketBase> translator = _mapperFactory.GetInstance<TransactionalBlock, PacketBase>();

                    PacketBase block = translator?.Translate(transactionalBlock);

                    blocks.Add(block as T);
                }
            }

            return blocks;
        }

        public List<PacketBase> GetAllLastBlocksByType(ushort blockType)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PacketBase> GetAllByKey(IKey key)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PacketBase> GetAll()
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public PacketBase Get(IDataKey key)
        {
            if(key is SyncHashKey syncHashKey)
            {
                TransactionalBlock transactionalBlock = DataAccessService.Instance.GetTransactionalBySyncAndHash(syncHashKey.SyncBlockHeight, syncHashKey.Hash);

                return _mapperFactory.GetInstance<TransactionalBlock, PacketBase>().Translate(transactionalBlock);
            }

            return null;
        }

        public void Update(IDataKey key, PacketBase item)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PacketBase> GetAll(IDataKey key)
        {
            throw new NotImplementedException();
        }
    }
}
