using System.IO;
using Wist.Blockchain.Core.DataModel.Synchronization;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Blockchain.Core.Serializers.Signed.Synchronization
{
    [RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class SynchronizationProducingBlockSerializer : LinkedSerializerBase<SynchronizationProducingBlock>
    {
        public SynchronizationProducingBlockSerializer() : base(PacketType.Synchronization, BlockTypes.Synchronization_TimeSyncProducingBlock)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            base.WriteBody(bw);

            bw.Write(_block.ReportedTime.ToBinary());
            bw.Write(_block.Round);
        }
    }
}
