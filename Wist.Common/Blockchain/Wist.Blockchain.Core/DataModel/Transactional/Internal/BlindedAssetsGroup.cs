using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Blockchain.Core.DataModel.Transactional.Internal
{
    public class BlindedAssetsGroup
    {
        public uint GroupId { get; set; }

        public byte[][] AssetCommitments { get; set; }
    }
}
