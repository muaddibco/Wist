using CommonServiceLocator;
using System;
using System.IO;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Cryptography;

namespace Wist.BlockLattice.Core.Serializers.Signed
{
    public abstract class SignatureSupportSerializerBase<T> : ISignatureSupportSerializer where T : SignedBlockBase
    {
        public const byte DLE = 0x10;
        public const byte STX = 0x02;

        protected Lazy<ISignatureSupportSerializersFactory> _serializersFactory;

        protected T _block;
        protected readonly ICryptoService _cryptoService;

        protected readonly MemoryStream _memoryStream;
        protected readonly BinaryWriter _binaryWriter;
        protected readonly BinaryReader _binaryReader;


        private bool _disposed = false; // To detect redundant calls

        public SignatureSupportSerializerBase(PacketType packetType, ushort blockType, ICryptoService cryptoService)
        {
            _cryptoService = cryptoService;

            PacketType = packetType;
            BlockType = blockType;

            _memoryStream = new MemoryStream();
            _binaryWriter = new BinaryWriter(_memoryStream);
            _binaryReader = new BinaryReader(_memoryStream);

            _serializersFactory = new Lazy<ISignatureSupportSerializersFactory>(() => ServiceLocator.Current.GetInstance<ISignatureSupportSerializersFactory>());
        }

        public PacketType PacketType { get; }
        public ushort BlockType { get; }

        public virtual byte[] GetBytes()
        {
            if (_block == null)
            {
                return null;
            }

            FillBodyAndRowBytes();

            return _block.RawData;
        }

        public virtual void Initialize(SignedBlockBase signedBlockBase)
        {
            _disposed = false;
            _block = signedBlockBase as T;
        }

        protected virtual void WriteSyncHeader(BinaryWriter bw)
        {
        }

        protected abstract void WriteBody(BinaryWriter bw);

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _serializersFactory.Value.Utilize(this);
                }
                else
                {
                    _binaryReader.Dispose();
                    _binaryWriter.Dispose();
                    _memoryStream.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void FillBodyAndRowBytes()
        {
            if (_block == null)
            {
                return;
            }

            _memoryStream.Seek(0, SeekOrigin.Begin);
            _memoryStream.SetLength(0);

            _binaryWriter.Write((ushort)PacketType);

            WriteSyncHeader(_binaryWriter);

            long pos = _memoryStream.Position;

            _binaryWriter.Write(_block.Version);
            _binaryWriter.Write(_block.BlockType);

            WriteBody(_binaryWriter);

            long bodyLength = _memoryStream.Position - pos;
            _memoryStream.Seek(pos, SeekOrigin.Begin);

            byte[] body = _binaryReader.ReadBytes((int)bodyLength);

            _block.BodyBytes = body;

            byte[] signature = _cryptoService.Sign(body);

            _binaryWriter.Write(signature);
            _binaryWriter.Write(_cryptoService.Key.Value);

            _block.RawData = _memoryStream.ToArray();
        }

        ~SignatureSupportSerializerBase()
        {
            Dispose(false);
        }
        #endregion
    }
}
