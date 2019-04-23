using System;
using System.Collections.Generic;
using Wist.Blockchain.Core.DAL.Keys;
using Wist.Blockchain.Core.DataModel;
using Wist.Blockchain.Core.DataModel.UtxoConfidential;
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
    public class UtxoConfidentialDataService : IChainDataService
    {
        private readonly ITranslatorsRepository _mapperFactory;
        public PacketType PacketType => PacketType.UtxoConfidential;

        public UtxoConfidentialDataService(ITranslatorsRepository mapperFactory)
        {
            _mapperFactory = mapperFactory;
        }

        public void Add(PacketBase item)
        {
            if(item is UtxoConfidentialBase utxoConfidential)
            {
                DataAccessService.Instance.AddUtxoConfidentialBlock(utxoConfidential.KeyImage, utxoConfidential.SyncBlockHeight, utxoConfidential.BlockType, utxoConfidential.DestinationKey, utxoConfidential.RawData.ToArray());
            }
        }

        public bool AreServiceActionsAllowed(IKey key)
        {
            return !DataAccessService.Instance.IsUtxoConfidentialImageKeyExist(key);
        }

        public PacketBase Get(IDataKey key)
        {
            if (key is SyncHashKey syncHashKey)
            {
                UtxoConfidentialBlock utxoConfidential = DataAccessService.Instance.GetUtxoConfidentialBySyncAndHash(syncHashKey.SyncBlockHeight, syncHashKey.Hash);

				if (utxoConfidential != null)
				{
					return _mapperFactory.GetInstance<UtxoConfidentialBlock, PacketBase>().Translate(utxoConfidential);
				}
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
