using CommonServiceLocator;
using System;
using System.IO;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Identity;

namespace Wist.BlockLattice.Core.Serializers
{
    public abstract class SerializerBase<T> : ISerializer where T : BlockBase
    {
        public const byte DLE = 0x10;
        public const byte STX = 0x02;

        protected Lazy<ISerializersFactory> _serializersFactory;

        protected readonly MemoryStream _memoryStream;
        protected readonly BinaryWriter _binaryWriter;
        protected readonly BinaryReader _binaryReader;

        protected bool _bytesFilled;
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

        public abstract void FillBodyAndRowBytes();

        public virtual byte[] GetBytes()
        {
            if (_block == null)
            {
                return null;
            }

            FillBodyAndRowBytes();

            return _block.RawData.ToArray();
        }

        public virtual void Initialize(BlockBase blockBase)
        {
            _bytesFilled = false;
            _disposed = false;
            _block = blockBase as T;
        }

        public IKey GetKey()
        {
            if (_block == null)
            {
                return null;
            }

            FillBodyAndRowBytes();

            return _block.Key;
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
