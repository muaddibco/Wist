using Chaos.NaCl;
using System.Collections.Generic;
using Wist.BlockLattice.Core;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Identity;
using Wist.Node.Core.Registry;

namespace Wist.Node.Core.Tests
{
    public static class PacketsBuilder
    {
        public static RegistryRegisterBlock GetTransactionRegisterBlock(ulong syncBlockHeight, uint nonce, byte[] powHash, ulong blockHeight, PacketType referencedPacketType, 
            ushort referencedBlockType, byte[] referencedBlockHash, byte[] referencedTarget, byte[] privateKey)
        {
            byte[] publicKey = Ed25519.PublicKeyFromSeed(privateKey);
            RegistryRegisterBlock transactionRegisterBlock = new RegistryRegisterBlock
            {
                SyncBlockHeight = syncBlockHeight,
                Nonce = nonce,
                PowHash = powHash??new byte[Globals.POW_HASH_SIZE],
                BlockHeight = blockHeight,
                ReferencedPacketType = referencedPacketType,
                ReferencedBlockType = referencedBlockType,
                ReferencedBodyHash = referencedBlockHash,
                ReferencedTarget = referencedTarget,
                Signer = new Key32(publicKey)
            };

            return transactionRegisterBlock;
        }

        public static RegistryShortBlock GetTransactionsShortBlock(ulong syncBlockHeight, uint nonce, byte[] powHash, ulong blockHeight, byte round, IEnumerable<RegistryRegisterBlock> transactionRegisterBlocks, byte[] privateKey, ITransactionsRegistryHelper transactionsRegistryHelper)
        {
            byte[] publicKey = Ed25519.PublicKeyFromSeed(privateKey);

            SortedList<ushort, IKey> transactionHeaders = new SortedList<ushort, IKey>();

            ushort order = 0;
            foreach (var item in transactionRegisterBlocks)
            {
                transactionHeaders.Add(order++, transactionsRegistryHelper.GetTransactionRegistryTwiceHashedKey(item));
            }

            RegistryShortBlock transactionsShortBlock = new RegistryShortBlock
            {
                SyncBlockHeight = syncBlockHeight,
                Nonce = nonce,
                PowHash = powHash ?? new byte[Globals.POW_HASH_SIZE],
                BlockHeight = blockHeight,
                TransactionHeaderHashes = transactionHeaders,
                Signer = new Key32(publicKey)
            };

            return transactionsShortBlock;
        }
    }
}
