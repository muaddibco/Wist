using System.IO;
using Wist.Blockchain.Core.DataModel;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Models;

namespace Wist.Blockchain.Core.Serializers.Signed
{
    public abstract class SignatureSupportSerializerBase<T> : SerializerBase<T> where T : SignedPacketBase
    {
        public SignatureSupportSerializerBase(PacketType packetType, ushort blockType)
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

        protected abstract void WriteBody(BinaryWriter bw);

        public override void SerializeBody()
        {
            if (_block == null || _serializationBodyDone)
            {
                return;
            }

            FillHeader();

            FillBody();

            _serializationBodyDone = true;
        }

        public override void SerializeFully()
        {
            if (_block == null || _serializationFullyDone)
            {
                return;
            }

            SerializeBody();

            FinalizeTransaction();

            _serializationFullyDone = true;
        }

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
            _binaryWriter.Write(_block.BlockHeight);

            WriteBody(_binaryWriter);

            _block.BodyBytes = _memoryStream.ToArray();
        }

        private void FinalizeTransaction()
        {
            _binaryWriter.Write(_block.Signature.ToArray());
            _binaryWriter.Write(_block.Signer.Value.ToArray());
            _block.RawData = _memoryStream.ToArray();
        }
        
        #endregion Private Functions
    }
}
