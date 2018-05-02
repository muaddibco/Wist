using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel.Account
{
    public class AccountGenesisBlock : GenesisBlockBase
    {
        public override ChainType ChainType => ChainType.AccountChain;

        public override ushort Version => 1;
    }
}
