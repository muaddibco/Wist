using Chaos.NaCl;
using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.Core.Identity;

namespace Wist.Tests.Core
{
    public class PacketsBuilder
    {
        public static TransactionRegisterBlock GetTransactionRegisterBlock(ulong syncBlockHeight, uint nonce, byte[] powHash, ulong blockHeight, TransactionHeader transactionHeader, byte[] privateKey)
        {
            byte[] publicKey = Ed25519.PublicKeyFromSeed(privateKey);
            TransactionRegisterBlock transactionRegisterBlock = new TransactionRegisterBlock
            {
                SyncBlockHeight = syncBlockHeight,
                Nonce = nonce,
                HashNonce = powHash??new byte[Globals.POW_HASH_SIZE],
                BlockHeight = blockHeight,
                TransactionHeader = transactionHeader,
                Key = new Public32Key(publicKey)
            };

            return transactionRegisterBlock;
        }
    }
}
