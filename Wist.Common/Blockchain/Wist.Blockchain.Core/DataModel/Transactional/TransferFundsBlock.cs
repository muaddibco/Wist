using Wist.Blockchain.Core.Enums;

namespace Wist.Blockchain.Core.DataModel.Transactional
{
    public class TransferFundsBlock : TransactionalPacketBase
    {
        public override ushort BlockType => BlockTypes.Transaction_TransferFunds;

        public override ushort Version => 1;

        /// <summary>
        /// 32 byte of Original Hash value of Transactional Account that is target of transaction
        /// </summary>
        public byte[] TargetOriginalHash { get; set; }
    }
}
