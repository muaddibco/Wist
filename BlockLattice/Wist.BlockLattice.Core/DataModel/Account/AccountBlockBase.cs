﻿using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel.Account
{
    public abstract class AccountBlockBase : BlockBase
    {
        public override ChainType ChainType => ChainType.AccountChain;
    }
}
