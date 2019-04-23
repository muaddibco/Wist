using Wist.Blockchain.Core.Enums;

namespace Wist.Blockchain.Core.DataModel.Synchronization
{
    public class SynchronizationRegistryCombinedBlock : SynchronizationBlockBase
    {
        public override ushort BlockType => BlockTypes.Synchronization_RegistryCombinationBlock;

        public override ushort Version => 1;

        public byte[][] BlockHashes { get; set; }
    }
}
