﻿using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel.Transactional
{
    public class AcceptFundsBlock : TransactionalBlockBase
    {
        public override BlockType BlockType => BlockType.Transaction_AcceptFunds;

        public override ushort Version => 1;

        /// <summary>
        /// 32 byte of Original Hash value of Transactional Account that is source of transaction that Income Transaction relates to
        /// </summary>
        public byte[] SourceOriginalHash { get; set; }
    }
}
