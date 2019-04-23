using Wist.Core.Models;

namespace Wist.Blockchain.Core.DataModel.UtxoConfidential
{
    public abstract class UtxoConfidentialBase : UtxoSignedPacketBase
    {
        public override ushort PacketType => (ushort)Enums.PacketType.UtxoConfidential;

        /// <summary>
        /// P = Hs(r * A) * G + B where A is receiver's Public View Key and B is receiver's Public Spend Key
        /// </summary>
        public byte[] DestinationKey { get; set; }

		/// <summary>
		/// This is destination key of target that sender wants to authorize with
		/// </summary>
		public byte[] DestinationKey2 { get; set; }

		/// <summary>
		/// R = r * G. 'r' can be erased after transaction sent unless sender wants to proof he sent funds to particular destination address.
		/// </summary>
		public byte[] TransactionPublicKey { get; set; }
	}
}
