using Wist.Blockchain.Core.DataModel.Transactional.Internal;
using Wist.Blockchain.Core.Enums;

namespace Wist.Blockchain.Core.DataModel.Transactional
{
    public class IssueGroupedAssets : TransactionalPacketBase
    {
        public override ushort Version => 1;

        public override ushort BlockType => BlockTypes.Transaction_IssueGroupedAssets;

        /// <summary>
        /// Contains non-blinded assets just being issued. These assets are devoted for referencing
        /// </summary>
        public AssetsIssuanceGroup[] AssetsIssuanceGroups { get; set; }

        /// <summary>
        /// Contains blinded assets just being issued. These assets are devoted for owning
        /// </summary>
        public BlindedAssetsIssuanceGroup[] BlindedAssetsIssuanceGroups { get; set; }

        public string IssuanceInfo { get; set; }

        /// <summary>
        /// Contains latest state of all groups of non-blinded assets that new assets were issued for
        /// </summary>
        public AssetsGroup[] AssetsGroups { get; set; }

        /// <summary>
        /// Contains latest state of all groups of blinded assets that new assets were issued for
        /// </summary>
        public BlindedAssetsGroup[] BlindedAssetsGroups { get; set; }
    }
}
