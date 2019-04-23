using System;
using System.IO;
using Wist.Blockchain.Core.DataModel.UtxoConfidential;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Cryptography;
using Wist.Core.Identity;

namespace Wist.Blockchain.Core.Serializers.UtxoConfidential
{
    public abstract class UtxoConfidentialSerializerBase<T> : SerializerBase<T> where T : UtxoConfidentialBase
    {
        protected int _prevSecretKeyIndex;
        protected byte[] _prevSecretKey;

        public UtxoConfidentialSerializerBase(PacketType packetType, ushort blockType) 
            : base(packetType, blockType)
        {
        }

        protected virtual void WriteHeader(BinaryWriter bw)
        {
            bw.Write((ushort)PacketType);
            bw.Write(_block.SyncBlockHeight);
            bw.Write(_block.Nonce);
            bw.Write(_block.PowHash);
        }

        public override void SerializeBody()
        {
            if (_block == null || _serializationFullyDone)
            {
                return;
            }

            FillHeader();

            FillBody();
        }

        public override void SerializeFully()
        {
            if (_block == null || _serializationFullyDone)
            {
                return;
            }

            FillHeader();

            FillBody();

            FinalizeTransaction();

            _serializationFullyDone = true;
        }

        protected abstract void WriteBody(BinaryWriter bw);

        #region Private Functions

        private void FillHeader()
        {
            _memoryStream.Seek(0, SeekOrigin.Begin);
            _memoryStream.SetLength(0);

            WriteHeader(_binaryWriter);
        }

        private void FillBody()
        {
            _binaryWriter.Write(_block.Version);
            _binaryWriter.Write(_block.BlockType);
            _binaryWriter.Write(_block.DestinationKey);
            _binaryWriter.Write(_block.DestinationKey2);
			_binaryWriter.Write(_block.TransactionPublicKey);

            WriteBody(_binaryWriter);

            _block.BodyBytes = _memoryStream.ToArray();
        }

        private void FinalizeTransaction()
        {
            _binaryWriter.Write(_block.KeyImage.Value.ToArray());

            RingSignature[] ringSignatures = _block.Signatures;

            _binaryWriter.Write((ushort)ringSignatures.Length);

            foreach (IKey key in _block.PublicKeys)
            {
                _binaryWriter.Write(key.ArraySegment.Array, key.ArraySegment.Offset, key.ArraySegment.Count);
            }

            foreach (var signature in ringSignatures)
            {
                _binaryWriter.Write(signature.C);
                _binaryWriter.Write(signature.R);
            }

            Memory<byte> memory = _memoryStream.ToArray();

            _block.RawData = memory;
        }

        #endregion Private Functions


        protected static void WriteSurjectionProof(BinaryWriter bw, SurjectionProof surjectionProof)
        {
            bw.Write((ushort)surjectionProof.AssetCommitments.Length);
            for (int i = 0; i < surjectionProof.AssetCommitments.Length; i++)
            {
                bw.Write(surjectionProof.AssetCommitments[i]);
            }

            bw.Write(surjectionProof.Rs.E);

            for (int i = 0; i < surjectionProof.AssetCommitments.Length; i++)
            {
                bw.Write(surjectionProof.Rs.S[i]);
            }
        }

        protected static void WriteBorromeanRingSignature(BinaryWriter bw, BorromeanRingSignature borromeanRingSignature)
        {
            bw.Write(borromeanRingSignature.E);
            bw.Write((ushort)borromeanRingSignature.S.Length);
            for (int i = 0; i < borromeanRingSignature.S.Length; i++)
            {
                bw.Write(borromeanRingSignature.S[i]);
            }
        }

        protected static void WriteEcdhTupleCA(BinaryWriter bw, EcdhTupleCA ecdhTuple)
        {
            bw.Write(ecdhTuple.Mask);
            bw.Write(ecdhTuple.AssetId);
        }

		protected static void WriteEcdhTupleIP(BinaryWriter bw, EcdhTupleIP ecdhTuple)
		{
			bw.Write(ecdhTuple.Issuer);
			bw.Write(ecdhTuple.Payload);
		}

		protected static void WriteEcdhTupleProofs(BinaryWriter bw, EcdhTupleProofs ecdhTuple)
        {
            bw.Write(ecdhTuple.Mask);
            bw.Write(ecdhTuple.AssetId);
            bw.Write(ecdhTuple.AssetIssuer);
            bw.Write(ecdhTuple.Payload);
        }
    }
}
