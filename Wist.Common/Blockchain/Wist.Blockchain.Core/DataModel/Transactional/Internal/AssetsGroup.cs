using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Blockchain.Core.DataModel.Transactional.Internal
{
    public class AssetsGroup
    {
        public uint GroupId { get; set; }

        public byte[][] AssetIds { get; set; }

        public ulong[] AssetAmounts { get; set; }
    }
}
