using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Blockchain.Core.DataModel.Transactional.Internal
{
    public class AssetsIssuanceGroup
    {
        public uint GroupId { get; set; }

        public AssetIssuance[] AssetIssuances { get; set; }
    }
}
