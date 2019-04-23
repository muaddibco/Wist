using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Blockchain.Core.DataModel.Transactional.Internal
{
    public class AcceptedAssetUnblinding
    {
        public byte[] AssetId { get; set; }

        public byte[] BlindingFactor { get; set; }
    }
}
