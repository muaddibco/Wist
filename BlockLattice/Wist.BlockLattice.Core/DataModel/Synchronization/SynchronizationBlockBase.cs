﻿using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel.Synchronization
{
    public abstract class SynchronizationBlockBase : SignedBlockBase
    {
        public override ChainType ChainType => ChainType.Synchronization;

        public DateTime ReportedTime { get; set; }
    }
}