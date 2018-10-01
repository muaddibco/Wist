using Wist.BlockLattice.Core.Enums;
using Wist.Core.Identity;

namespace Wist.BlockLattice.Core.DataModel.Registry
{

    public class RegistryConfirmationBlock : RegistryBlockBase
    {
        public override ushort BlockType => BlockTypes.Registry_ConfirmationBlock;

        public override ushort Version => 1;

        public byte[] ReferencedBlockHash { get; set; }

        //public class ConfidenceDescriptor
        //{
        //    public ushort Confidence { get; set; }

        //    public byte[] Signature{ get; set; }

        //    public IKey Signer { get; set; }
        //}
    }
}
