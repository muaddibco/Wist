using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel.Registry
{
    public class TransactionsRegistryConfidenceBlock : RegistryBlockBase
    {
        public override ushort BlockType => BlockTypes.Registry_ConfidenceBlock;

        public override ushort Version => 1;

        public ushort Confidence { get; set; }

        public byte[] ReferencedBlockHash { get; set; }
    }
}
