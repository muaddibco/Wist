    using Wist.BlockLattice.Core.DataModel.UtxoConfidential;
using Wist.Core.Identity;

namespace Wist.BlockLattice.Core.DataModel.Registry.SourceKeys
{
    public class UtxoConfidentialSourceKey : ITransactionSourceKey<UtxoConfidentialSourceKey>
    {
        public UtxoConfidentialSourceKey(UtxoConfidentialBase utxoConfidentialBase)
        {
            KeyImage = utxoConfidentialBase.KeyImage;
        }

        public IKey KeyImage { get; private set; }

        public bool Equals(UtxoConfidentialSourceKey other)
        {
            return KeyImage.Equals(other.KeyImage);
        }

        public bool Equals(UtxoConfidentialSourceKey x, UtxoConfidentialSourceKey y)
        {
            return x.KeyImage.Equals(y.KeyImage);
        }

        public int GetHashCode(UtxoConfidentialSourceKey obj)
        {
            return KeyImage.GetHashCode();
        }
    }
}
