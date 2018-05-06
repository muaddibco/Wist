using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel.Transactional
{
    public class TransferFundsBlockV1 : TransactionalBlockBaseV1
    {
        public override ushort BlockType => BlockTypes.Transaction_TransferFunds;

        /// <summary>
        /// 32 byte of Original Hash value of Transactional Account that is target of transaction
        /// </summary>
        public byte[] TargetOriginalHash { get; set; }
    }
}
