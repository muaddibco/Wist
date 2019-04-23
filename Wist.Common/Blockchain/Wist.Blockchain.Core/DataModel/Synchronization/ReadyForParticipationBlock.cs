using Wist.Blockchain.Core.Enums;

namespace Wist.Blockchain.Core.DataModel.Synchronization
{
    /// <summary>
    /// <see cref="ReadyForParticipationBlock"/> class required for letting know existing participants of Synchronization Group that new member is ready to participate in achieving consensus on Synchronization Block approval
    /// </summary>
    public class ReadyForParticipationBlock : SynchronizationBlockBase
    {
        public override ushort PacketType => (ushort)Enums.PacketType.Synchronization;

        public override ushort BlockType => BlockTypes.Synchronization_ReadyToParticipateBlock;

        public override ushort Version => 1;
    }
}
