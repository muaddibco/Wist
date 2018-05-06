using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.Parsers.Transactional
{
    public abstract class TransactionalBlockParserBase : BlockParserBase
    {
        public override ChainType ChainType => ChainType.TransactionalChain;
    }
}
