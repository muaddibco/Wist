using CommonServiceLocator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Cryptography;
using Wist.Core.States;
using Wist.Core.Synchronization;

namespace Wist.BlockLattice.Core.Serializers.Signed
{
    public abstract class SignatureSupportSerializerBase<T> : ISignatureSupportSerializer where T : SignedBlockBase
    {
        public const byte DLE = 0x10;
        public const byte STX = 0x02;

        private Lazy<ISignatureSupportSerializersFactory> _serializersFactory;

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

            _memoryStream.Seek(0, SeekOrigin.Begin);
            _memoryStream.SetLength(0);

            _binaryWriter.Write((ushort)PacketType);

            WriteSyncHeader(_binaryWriter);

            _binaryWriter.Write(_block.Version);
            _binaryWriter.Write(_block.BlockType);

            long pos = _memoryStream.Position;

            WriteBody(_binaryWriter);

            long bodyLength = _memoryStream.Position - pos;
            _memoryStream.Seek(pos, SeekOrigin.Begin);

            byte[] body = _binaryReader.ReadBytes((int)bodyLength);

            byte[] signature = _cryptoService.Sign(body);

            _binaryWriter.Write(signature);
            _binaryWriter.Write(_block.Key.Value);

            return _memoryStream.ToArray();
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

        ~SignatureSupportSerializerBase()
        {
            Dispose(false);
        }
        #endregion
    }
}
