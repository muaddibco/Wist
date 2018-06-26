using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.BlockLattice.Core.DataModel.Transactional
{
    public class TransactionalBlockchain
    {
        /// <summary>
        /// The very first block that entire transactional blockchain starts from
        /// </summary>
        public TransactionalGenesisBlock GenesisBlock { get; }

        /// <summary>
        /// Collection of all blocks of transactional blockchain ordered by their BlockOrder value. 
        /// Values of sorted list are <see cref="HashSet<TransactionalBlockBase>"/> that is collection of 
        /// blocks with them same BlockOrder appeared because of unconventional forks
        /// </summary>
        public SortedList<uint, HashSet<TransactionalBlockBase>> Blocks { get; }

        /// <summary>
        /// Contains collection of blocks that are representing end of blockchain. In case when there are forks collection will contain more than 1 element.
        /// </summary>
        public HashSet<TransactionalBlockBase> ForkEnds { get; set; }
    }
}
