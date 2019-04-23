using Chaos.NaCl;
using System.Collections.Generic;
using System.Linq;
using Wist.Blockchain.Core;
using Wist.Blockchain.Core.DataModel.Registry;
using Wist.Blockchain.Core.Enums;
using Wist.Core;
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

        public static RegistryShortBlock GetTransactionsShortBlock(ulong syncBlockHeight, uint nonce, byte[] powHash, ulong blockHeight, byte round, IEnumerable<RegistryRegisterBlock> transactionRegisterBlocks, byte[] privateKey)
        {
            byte[] publicKey = Ed25519.PublicKeyFromSeed(privateKey);

            WitnessStateKey[] transactionHeaders = new WitnessStateKey[transactionRegisterBlocks.Count()];

            ushort order = 0;
            foreach (var item in transactionRegisterBlocks)
            {
                transactionHeaders[order++] = new WitnessStateKey { PublicKey = item.Signer, Height = item.BlockHeight };
            }

            RegistryShortBlock transactionsShortBlock = new RegistryShortBlock
            {
                SyncBlockHeight = syncBlockHeight,
                Nonce = nonce,
                PowHash = powHash ?? new byte[Globals.POW_HASH_SIZE],
                BlockHeight = blockHeight,
                WitnessStateKeys = transactionHeaders,
                Signer = new Key32(publicKey)
            };

            return transactionsShortBlock;
        }
    }
}
