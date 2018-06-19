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

        protected T _block;
        protected readonly ICryptoService _cryptoService;
        protected readonly IAccountState _accountState;
        protected readonly ISynchronizationContext _synchronizationContext;

        protected readonly MemoryStream _memoryStream;
        protected readonly BinaryWriter _binaryWriter;
        protected readonly BinaryReader _binaryReader;

        private bool _disposed = false; // To detect redundant calls

        public SignatureSupportSerializerBase(PacketType packetType, ushort blockType, ICryptoService cryptoService, IStatesRepository statesRepository)
        {
            _cryptoService = cryptoService;
            _accountState = statesRepository.GetInstance<IAccountState>();
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
            PacketType = packetType;
            BlockType = blockType;

            _memoryStream = new MemoryStream();
            _binaryWriter = new BinaryWriter(_memoryStream);
            _binaryReader = new BinaryReader(_memoryStream);
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
            _binaryWriter.Write(_synchronizationContext.LastBlockDescriptor.BlockHeight);

            WritePowHeader(_binaryWriter, _synchronizationContext.LastBlockDescriptor.BlockHeight);

            _binaryWriter.Write(_block.BlockType);
            _binaryWriter.Write(_block.Version);

            long pos = _memoryStream.Position;

            WriteBody(_binaryWriter);

            long bodyLength = _memoryStream.Position - pos;
            _memoryStream.Seek(pos, SeekOrigin.Begin);

            byte[] body = _binaryReader.ReadBytes((int)bodyLength);

            byte[] signature = _cryptoService.Sign(body);

            _binaryWriter.Write(signature);
            _binaryWriter.Write(_accountState.PublicKey);

            return _memoryStream.ToArray();
        }

        public virtual void Initialize(SignedBlockBase signedBlockBase)
        {
            _block = signedBlockBase as T;
        }

        protected abstract void WritePowHeader(BinaryWriter bw, uint syncBlockHeight);

        protected abstract void WriteBody(BinaryWriter bw);

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
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
        #endregion
    }
}
