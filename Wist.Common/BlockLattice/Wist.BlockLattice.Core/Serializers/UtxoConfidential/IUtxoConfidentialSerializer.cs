using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel.UtxoConfidential;
using Wist.Core.Identity;

namespace Wist.BlockLattice.Core.Serializers.UtxoConfidential
{
    public interface IUtxoConfidentialSerializer : ISerializer
    {
        void Initialize(UtxoConfidentialBase utxoConfidentialBase, IKey receiverViewKey, IKey receiverSpendKey, byte[] prevSecretKey, int prevSecretKeyIndex);
    }
}
