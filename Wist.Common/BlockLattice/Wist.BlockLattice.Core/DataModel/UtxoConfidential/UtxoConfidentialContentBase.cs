using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.BlockLattice.Core.DataModel.UtxoConfidential
{
    public abstract class UtxoConfidentialContentBase : UtxoConfidentialBase
    {
        public byte[] TransactionPublicKey { get; set; }
    }
}
