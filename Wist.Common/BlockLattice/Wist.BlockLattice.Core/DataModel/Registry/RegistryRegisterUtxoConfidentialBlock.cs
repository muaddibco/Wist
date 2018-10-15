using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel.UtxoConfidential;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel.Registry
{
    public class RegistryRegisterUtxoConfidentialBlock : UtxoConfidentialBase
    {
        public override PacketType PacketType => PacketType.Registry;

        public override ushort Version => 1;

        public override ushort BlockType => BlockTypes.Registry_RegisterUtxoConfidential;
    }
}
