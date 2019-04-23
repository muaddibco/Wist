using System;
using System.Collections.Generic;
using System.Text;
using Wist.Blockchain.Core.DataModel.Transactional.Internal;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Cryptography;
using Wist.Core.Identity;

namespace Wist.Blockchain.Core.DataModel.Transactional
{
    /// <summary>
    /// Accepts assets from UTXO transaction with adding value commitment
    /// </summary>
    public class AcceptAssets : TransactionalPacketBase
    {
        public override ushort Version => 1;

        public override ushort BlockType => BlockTypes.Transaction_AcceptAssets;

        public AcceptedAssetUnblindingAB[] AcceptedAssetUnblindingABs { get; set; }

        public AcceptedAssetUnblindingUtxo[] AcceptedAssetUnblindingUtxos { get; set; }

        public AssetsGroup[] AssetsGroups { get; set; }
    }
}
