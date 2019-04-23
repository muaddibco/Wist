using System.Collections.Generic;
using Wist.Blockchain.Core.Enums;

namespace Wist.Blockchain.Core.DataModel.Registry
{
    public class RegistryFullBlock : RegistryBlockBase
    {
        public override ushort BlockType => BlockTypes.Registry_FullBlock;

        public override ushort Version => 1;

        public RegistryRegisterBlock[] StateWitnesses { get; set; }
        public RegistryRegisterUtxoConfidential[] UtxoWitnesses { get; set; }

        public byte[] ShortBlockHash { get; set; }
    }
}
