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
        protected T _block;
        protected readonly ICryptoService _cryptoService;
        protected readonly IAccountState _accountState;
        protected readonly ISynchronizationContext _synchronizationContext;

        public SignatureSupportSerializerBase(PacketType packetType, ushort blockType, ICryptoService cryptoService, IStatesRepository statesRepository)
        {
            _cryptoService = cryptoService;
            _accountState = statesRepository.GetInstance<IAccountState>();
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
            PacketType = packetType;
            BlockType = blockType;
        }

        public PacketType PacketType { get; }
        public ushort BlockType { get; }

        public virtual byte[] GetBytes()
        {
            if (_block == null)
            {
                return null;
            }

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write((ushort)PacketType);
                    bw.Write(_synchronizationContext.LastBlockDescriptor.BlockHeight);

                    WriteBody(bw);
                }

                return ms.ToArray();
            }
        }

        public virtual void Initialize(SignedBlockBase signedBlockBase)
        {
            _block = signedBlockBase as T;
        }

        protected abstract void WritePowHeader(BinaryWriter bw);

        protected abstract byte[] WriteBody(BinaryWriter bw);
    }
}
