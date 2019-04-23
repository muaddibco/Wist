using System;
using System.Collections.Generic;
using System.Text;
using Wist.Blockchain.Core.DataModel.Transactional.Internal;
using Wist.Blockchain.Core.Enums;

namespace Wist.Blockchain.Core.DataModel.Transactional
{
    public class TransferGroupedAssets : TransactionalPacketBase
    {
        public override ushort Version => 1;

        public override ushort BlockType => BlockTypes.Transaction_TransferGroupedAssets;

        /// <summary>
        /// Contains hash of Public Key of the receiver
        /// </summary>
        public byte[] Target { get; set; }

        public EncryptedAsset[] TransferedAssets { get; set; }

        /// <summary>
        /// Contains list of all assets remained at this account
        /// </summary>
        public BlindedAssetsGroup[] BlindedAssetsGroups { get; set; }

        /// <summary>
        /// Inversed Surjection Proofs must contain assets from all groups listed in transaction and all assets transferred to another account for proving that assets were not changed.
        /// </summary>
        public InversedSurjectionProof[] InversedSurjectionProofs { get; set; }
    }
}
