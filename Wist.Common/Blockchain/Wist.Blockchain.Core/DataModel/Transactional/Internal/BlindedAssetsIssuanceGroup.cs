using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Blockchain.Core.DataModel.Transactional.Internal
{
    public class BlindedAssetsIssuanceGroup : BlindedAssetsGroup
    {
        /// <summary>
        /// List of pairs of issued Blinded Asset Ids + Asset Issuance Info
        /// </summary>
        public AssetIssuance[] AssetIssuances { get; set; }

        /// <summary>
        /// Proofs for each AssetCommitment that it commits to issued asset id
        /// </summary>
        public IssuanceProof[] IssuanceProofs { get; set; }
    }
}
