using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel.Registry
{
    public class RegistryConfidenceBlock : RegistryBlockBase
    {
        public override ushort BlockType => BlockTypes.Registry_ConfidenceBlock;

        public override ushort Version => 1;

        public byte[] BitMask { get; set; }

        public byte[] ConfidenceProof { get; set; }

        public byte[] ReferencedBlockHash { get; set; }
    }
}
