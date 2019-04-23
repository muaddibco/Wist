using CommonServiceLocator;
using System;
using System.IO;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Identity;
using Wist.Core.Models;

namespace Wist.Blockchain.Core.Serializers
{
    public abstract class SerializerBase<T> : ISerializer where T : PacketBase
    {
        public const byte DLE = 0x10;
        public const byte STX = 0x02;

        protected Lazy<ISerializersFactory> _serializersFactory;

        protected readonly MemoryStream _memoryStream;
        protected readonly BinaryWriter _binaryWriter;
        protected readonly BinaryReader _binaryReader;

        protected bool _serializationBodyDone;
        protected bool _serializationFullyDone;
        protected T _block;

        private bool _disposed = false; // To detect redundant calls

        public SerializerBase(PacketType packetType, ushort blockType)
        {
            PacketType = packetType;
            BlockType = blockType;

            _memoryStream = new MemoryStream();
            _binaryWriter = new BinaryWriter(_memoryStream);
            _binaryReader = new BinaryReader(_memoryStream);

            _serializersFactory = new Lazy<ISerializersFactory>(() => ServiceLocator.Current.GetInstance<ISerializersFactory>());
        }

        public PacketType PacketType { get; }
        public ushort BlockType { get; }

        public abstract void SerializeBody();

        public abstract void SerializeFully();

        public virtual byte[] GetBytes()
        {
            if (_block == null)
            {
                return null;
            }

            SerializeFully();

            return _block.RawData.ToArray();
        }

        public virtual void Initialize(PacketBase blockBase)
        {
            _serializationBodyDone = false;
            _serializationFullyDone = false;
            _disposed = false;
            _block = blockBase as T;
        }

        public IKey GetKey()
        {
            if (_block == null)
            {
                return null;
            }

            SerializeFully();

            return null; // _block.Key;
        }

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

        ~SerializerBase()
        {
            Dispose(false);
        }
        #endregion
    }
}
