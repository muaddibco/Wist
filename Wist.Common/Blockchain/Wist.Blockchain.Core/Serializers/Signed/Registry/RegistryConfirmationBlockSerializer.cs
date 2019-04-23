using System.IO;
using Wist.Blockchain.Core.DataModel.Registry;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.HashCalculations;
using Wist.Core.Identity;

namespace Wist.Blockchain.Core.Serializers.Signed.Registry
{
    [RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class RegistryConfirmationBlockSerializer : SignatureSupportSerializerBase<RegistryConfirmationBlock>
    {
        public RegistryConfirmationBlockSerializer() : base(PacketType.Registry, BlockTypes.Registry_ConfirmationBlock)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            bw.Write(_block.ReferencedBlockHash);
        }
    }
}
