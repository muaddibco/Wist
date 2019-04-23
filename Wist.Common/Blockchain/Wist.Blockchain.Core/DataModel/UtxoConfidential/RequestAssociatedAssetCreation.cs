using System;
using System.Collections.Generic;
using System.Text;
using Wist.Blockchain.Core.Enums;

namespace Wist.Blockchain.Core.DataModel.UtxoConfidential
{
    /// <summary>
    /// Transaction intended for requesting creation of an associated asset
    /// </summary>
    public class RequestAssociatedAssetCreation : UtxoConfidentialBase
    {
        public override ushort Version => 1;

        public override ushort BlockType => BlockTypes.UtxoConfidential_RequestAssociatedAssetCreation;

        public byte[] IdentityCommitment { get; set; }

        /// <summary>
        /// 32-byte of address of the Identity Provider that provided identity was issued by
        /// </summary>
        public byte[] IdentityProvider { get; set; }


    }
}
