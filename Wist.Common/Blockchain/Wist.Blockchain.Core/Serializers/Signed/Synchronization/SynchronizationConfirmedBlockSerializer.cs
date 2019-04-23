using System.IO;
using Wist.Blockchain.Core.DataModel.Synchronization;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Blockchain.Core.Serializers.Signed.Synchronization
{
    [RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class SynchronizationConfirmedBlockSerializer : LinkedSerializerBase<SynchronizationConfirmedBlock>
    {
        public SynchronizationConfirmedBlockSerializer() : base(PacketType.Synchronization, BlockTypes.Synchronization_ConfirmedBlock)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            base.WriteBody(bw);

            bw.Write(_block.ReportedTime.ToBinary());
            bw.Write(_block.Round);
            byte signersCount = (byte)(_block.PublicKeys?.Length ?? 0);
            bw.Write(signersCount);
            for (int i = 0; i < signersCount; i++)
            {
                bw.Write(_block.PublicKeys[i]);
                bw.Write(_block.Signatures[i]);
            }
        }
    }
}
