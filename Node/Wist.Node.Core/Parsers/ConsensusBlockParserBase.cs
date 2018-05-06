using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Parsers;

namespace Wist.Node.Core.Parsers
{
    public abstract class ConsensusBlockParserBase : BlockParserBase
    {
        public override ChainType ChainType => ChainType.Consensus;
    }
}
