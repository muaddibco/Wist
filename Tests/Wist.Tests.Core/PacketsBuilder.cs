using Chaos.NaCl;
using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.Core.Cryptography;
using Wist.Core.Identity;

namespace Wist.Tests.Core
{
    public static class PacketsBuilder
    {
        public static RegistryRegisterBlock GetTransactionRegisterBlock(ulong syncBlockHeight, uint nonce, byte[] powHash, ulong blockHeight, TransactionHeader transactionHeader, byte[] privateKey)
        {
            byte[] publicKey = Ed25519.PublicKeyFromSeed(privateKey);
            RegistryRegisterBlock transactionRegisterBlock = new RegistryRegisterBlock
            {
                SyncBlockHeight = syncBlockHeight,
                Nonce = nonce,
                PowHash = powHash??new byte[Globals.POW_HASH_SIZE],
                BlockHeight = blockHeight,
                TransactionHeader = transactionHeader,
                Key = new Key32(publicKey)
            };

            return transactionRegisterBlock;
        }

        public static RegistryShortBlock GetTransactionsShortBlock(ulong syncBlockHeight, uint nonce, byte[] powHash, ulong blockHeight, byte round, IEnumerable<RegistryRegisterBlock> transactionRegisterBlocks, byte[] privateKey, ICryptoService cryptoService, IIdentityKeyProvider identityKeyProvider)
        {
            byte[] publicKey = Ed25519.PublicKeyFromSeed(privateKey);

            SortedList<ushort, IKey> transactionHeaders = new SortedList<ushort, IKey>();

            ushort order = 0;
            foreach (var item in transactionRegisterBlocks)
            {
                transactionHeaders.Add(order++, item.GetTransactionRegistryHashKey(cryptoService, identityKeyProvider));
            }

            RegistryShortBlock transactionsShortBlock = new RegistryShortBlock
            {
                SyncBlockHeight = syncBlockHeight,
                Nonce = nonce,
                PowHash = powHash ?? new byte[Globals.POW_HASH_SIZE],
                BlockHeight = blockHeight,
                TransactionHeaderHashes = transactionHeaders,
                Key = new Key32(publicKey)
            };

            return transactionsShortBlock;
        }
    }
}
