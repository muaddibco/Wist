using System.IO;
using Wist.Blockchain.Core.DataModel.Transactional;
using Wist.Blockchain.Core.DataModel.Transactional.Internal;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Cryptography;

namespace Wist.Blockchain.Core.Serializers.Signed.Transactional
{
    public abstract class TransactionalSerializerBase<T> : SignatureSupportSerializerBase<T> where T : TransactionalPacketBase
    {
        public TransactionalSerializerBase(PacketType packetType, ushort blockType) : base(packetType, blockType)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            bw.Write(_block.UptodateFunds);
        }

        protected static void WriteSurjectionProof(BinaryWriter bw, SurjectionProof surjectionProof)
        {
            bw.Write((ushort)surjectionProof.AssetCommitments.Length);
            for (int j = 0; j < surjectionProof.AssetCommitments.Length; j++)
            {
                bw.Write(surjectionProof.AssetCommitments[j]);
            }

            bw.Write(surjectionProof.Rs.E);
            for (int j = 0; j < surjectionProof.Rs.S.Length; j++)
            {
                bw.Write(surjectionProof.Rs.S[j]);
            }
        }

        protected static void WriteInversedSurjectionProof(BinaryWriter bw, InversedSurjectionProof surjectionProof)
        {
            bw.Write(surjectionProof.AssetCommitment);

            bw.Write(surjectionProof.Rs.E);
            bw.Write((ushort)surjectionProof.Rs.S.Length);
            for (int j = 0; j < surjectionProof.Rs.S.Length; j++)
            {
                bw.Write(surjectionProof.Rs.S[j]);
            }
        }
    }
}
