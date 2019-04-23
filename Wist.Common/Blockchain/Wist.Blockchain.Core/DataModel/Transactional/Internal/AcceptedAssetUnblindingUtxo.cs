using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Blockchain.Core.DataModel.Transactional.Internal
{
    public class AcceptedAssetUnblindingUtxo
    {
        public AcceptedAssetUnblinding AcceptedAssetsUnblinding { get; set; }

        /// <summary>
        /// Contains a Transaction Key specified in that transaction
        /// </summary>
        public byte[] SourceKey { get; set; }
    }
}
