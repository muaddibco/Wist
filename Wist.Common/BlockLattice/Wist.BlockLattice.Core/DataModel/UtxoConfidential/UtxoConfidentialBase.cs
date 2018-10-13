using Wist.BlockLattice.Core.Enums;
using Wist.Core.Identity;

namespace Wist.BlockLattice.Core.DataModel.UtxoConfidential
{
    public abstract class UtxoConfidentialBase : BlockBase
    {
        public override PacketType PacketType => PacketType.UtxoConfidential;

        public ulong SyncBlockHeight { get; set; }

        public uint Nonce { get; set; }

        /// <summary>
        /// 24 byte value of hash of sum of Hash of referenced Sync Block Content and Nonce
        /// </summary>
        public byte[] PowHash { get; set; }

        public IKey KeyImage { get; set; }

        public byte[] TransactionPublicKey { get; set; }

        public byte[] DestinationKey { get; set; }

        public IKey[] PublicKeys { get; set; }

        public Signature[] Signatures { get; set; }

        public class Signature
        {
            public Signature()
            {
            }

            /// <summary>
            /// 32 byte array
            /// </summary>
            public byte[] C { get; set; }

            /// <summary>
            /// 32 byte array
            /// </summary>
            public byte[] R { get; set; }
        }
    }
}
