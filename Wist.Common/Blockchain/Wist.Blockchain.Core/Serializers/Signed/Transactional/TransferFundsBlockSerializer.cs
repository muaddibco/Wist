using System.IO;
using Wist.Blockchain.Core.DataModel.Transactional;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Blockchain.Core.Serializers.Signed.Transactional
{
    [RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class TransferFundsBlockSerializer : TransactionalSerializerBase<TransferFundsBlock>
    {
        public TransferFundsBlockSerializer() : base(PacketType.Transactional, BlockTypes.Transaction_TransferFunds)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            base.WriteBody(bw);

            bw.Write(_block.TargetOriginalHash);
        }
    }
}
