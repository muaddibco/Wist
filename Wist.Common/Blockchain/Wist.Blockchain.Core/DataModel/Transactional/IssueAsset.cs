using Wist.Blockchain.Core.DataModel.Transactional.Internal;
using Wist.Blockchain.Core.Enums;

namespace Wist.Blockchain.Core.DataModel.Transactional
{
    public class IssueAsset : TransactionalPacketBase
    {
        public override ushort Version => 1;

        public override ushort BlockType => BlockTypes.Transaction_IssueAsset;

        /// <summary>
        /// can be real asset id like social security number or asset id that is pseudonym of real asset that is mapped in external database
        /// </summary>
        public AssetIssuance AssetIssuance { get; set; }
    }
}
