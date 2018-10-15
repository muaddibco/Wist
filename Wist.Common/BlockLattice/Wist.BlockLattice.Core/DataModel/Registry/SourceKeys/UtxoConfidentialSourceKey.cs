    using Wist.BlockLattice.Core.DataModel.UtxoConfidential;
using Wist.Core.Identity;

namespace Wist.BlockLattice.Core.DataModel.Registry.SourceKeys
{
    public class UtxoConfidentialSourceKey : ITransactionSourceKey<UtxoConfidentialSourceKey>
    {
        private readonly UtxoConfidentialBase _utxoConfidentialBase;

        public UtxoConfidentialSourceKey(UtxoConfidentialBase utxoConfidentialBase)
        {
            _utxoConfidentialBase = utxoConfidentialBase;
        }

        public IKey KeyImage => _utxoConfidentialBase.KeyImage;

        public bool Equals(ITransactionSourceKey x, ITransactionSourceKey y)
        {
            if(x == null && y == null)
            {
                return true;
            }

            if(x is UtxoConfidentialSourceKey utxoConfidentialSourceKey1 && y is UtxoConfidentialSourceKey utxoConfidentialSourceKey2)
            {
                return utxoConfidentialSourceKey1.KeyImage.Equals(utxoConfidentialSourceKey2.KeyImage);
            }

            return false;
        }

        public bool Equals(ITransactionSourceKey other)
        {
            if(other is UtxoConfidentialSourceKey utxoConfidentialSourceKey)
            {
                return KeyImage.Equals(utxoConfidentialSourceKey.KeyImage);
            }

            return false;
        }

        public int GetHashCode(ITransactionSourceKey obj)
        {
            if(obj is UtxoConfidentialSourceKey utxoConfidentialSourceKey)
            {
                return utxoConfidentialSourceKey.KeyImage.GetHashCode();
            }

            return 0;
        }

        public override int GetHashCode()
        {
            return KeyImage.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is UtxoConfidentialSourceKey utxoConfidentialSourceKey)
            {
                return KeyImage.Equals(utxoConfidentialSourceKey.KeyImage);
            }

            return false;
        }
    }
}
