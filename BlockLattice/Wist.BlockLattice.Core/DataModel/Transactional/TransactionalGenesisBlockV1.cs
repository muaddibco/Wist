using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel.Transactional
{
    /// <summary>
    /// Structure of block:
    ///   1. DLE + STX
    ///   2. LENGTH
    ///   3. CHAIN TYPE + MESSAGE TYPE
    ///   4. VERSION
    ///   5. Original Hash
    ///   6. Number of verifiers
    ///   7. Verifier Original Hash (N)
    ///   8. Recovery Original Hash
    /// </summary>
    public class TransactionalGenesisBlockV1 : GenesisBlockBase
    {
        // Genesis block and any other block must contain value of POW HASH based on content of Synchronization Blocks
        public TransactionalGenesisBlockV1()
        {
            VerifierOriginalHashList = new List<byte[]>();
        }

        public override ChainType ChainType => ChainType.TransactionalChain;

        public override ushort Version => 1;

        /// <summary>
        /// 64 byte of Hash value level N of account name encoded with Parent Account's Public Key
        /// </summary>
        public byte[] OriginalHash { get; set; }

        /// <summary>
        /// 64 byte of Hash value level N of account name encoded with Verifier Account's Public Key
        /// </summary>
        public List<byte[]> VerifierOriginalHashList { get; set; }

        /// <summary>
        /// 64 byte of Hash value level L of _unencrypted_ recovery passphrase
        /// </summary>
        public byte[] RecoveryOriginalHash { get; set; }

        /// <summary>
        /// 32 byte of Public Key of Node that this account votes for
        /// </summary>
        public byte[] NodeDpos { get; set; }
    }
}
