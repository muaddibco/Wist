using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wist.Crypto.Experiment.ConfidentialAssets
{
    public class EncryptedAssetID
    {
        byte[] _assetId;
        byte[] _blindingFactor;

        public EncryptedAssetID()
        {
            _assetId = new byte[32];
            _blindingFactor = new byte[32];
        }

        public byte[] AssetId { get => _assetId; set => _assetId = value; }
        public byte[] BlindingFactor { get => _blindingFactor; set => _blindingFactor = value; }
    }
}
