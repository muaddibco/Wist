using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wist.Blockchain.DataModel;
using Wist.Core.ExtensionMethods;
using Wist.Core.Identity;

namespace Wist.Blockchain.SQLite.DataAccess
{
    public partial class DataAccessService
    {
        #region Utxo Confidential

        private HashSet<IKey> _keyImages = new HashSet<IKey>(new Key32());

        public void LoadAllImageKeys()
        {
            _keyImages = new HashSet<IKey>(_dataContext.UtxoConfidentialKeyImages.Select(k => _identityKeyProvider.GetKey(k.KeyImage)).AsEnumerable(), new Key32());
        }

        public bool IsUtxoConfidentialImageKeyExist(IKey keyImage)
        {
            return _keyImages.Contains(keyImage);
        }

        public bool AddUtxoConfidentialBlock(IKey keyImage, ulong syncBlockHeight, ushort blockType, byte[] destinationKey, byte[] blockContent)
        {
            if(_keyImages.Contains(keyImage))
            {
                return false;
            }

            _keyImages.Add(keyImage);

            lock(_sync)
            {
                UtxoConfidentialKeyImage utxoConfidentialKeyImage = new UtxoConfidentialKeyImage { KeyImage = keyImage.Value.ToArray() };

                BlockHashKey blockHashKey = new BlockHashKey
                {
                SyncBlockHeight = syncBlockHeight,
                    Hash = _defaultHashCalculation.CalculateHash(blockContent)
                };

                UtxoConfidentialBlock utxoConfidentialBlock = new UtxoConfidentialBlock
                {
                    KeyImage = utxoConfidentialKeyImage,
                    HashKey = blockHashKey,
                    SyncBlockHeight = syncBlockHeight,
                    BlockType = blockType,
                    DestinationKey = destinationKey,
                    BlockContent = blockContent
                };

                _dataContext.UtxoConfidentialKeyImages.Add(utxoConfidentialKeyImage);
                _dataContext.BlockHashKeys.Add(blockHashKey);
                _dataContext.UtxoConfidentialBlocks.Add(utxoConfidentialBlock);
            }

            return true;
        }

        public UtxoConfidentialBlock GetUtxoConfidentialBySyncAndHash(ulong syncBlockHeight, Memory<byte> hash)
        {
			lock (_sync)
			{
				IEnumerable<BlockHashKey> blockHashKeys = _dataContext.BlockHashKeys.Where(b => b.SyncBlockHeight == syncBlockHeight);

				foreach (BlockHashKey item in blockHashKeys)
				{
					ArraySegment<byte> arraySegment = hash.ToArraySegment();

					if (item.Hash.Equals32(arraySegment.Array, arraySegment.Offset, 32))
					{
						UtxoConfidentialBlock utxoConfidentialBlock = _dataContext.UtxoConfidentialBlocks.FirstOrDefault(b => b.HashKey.BlockHashKeyId == item.BlockHashKeyId);

						return utxoConfidentialBlock;
					}
				}
			}

            return null;
        }
        #endregion Utxo Confidential
    }
}
