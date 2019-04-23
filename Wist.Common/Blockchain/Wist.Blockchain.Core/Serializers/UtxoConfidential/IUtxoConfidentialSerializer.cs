using Wist.Blockchain.Core.DataModel.UtxoConfidential;

namespace Wist.Blockchain.Core.Serializers.UtxoConfidential
{
    public interface IUtxoConfidentialSerializer : ISerializer
    {
        void Initialize(UtxoConfidentialBase utxoConfidentialBase, byte[] prevSecretKey, int prevSecretKeyIndex);
    }
}
