using System.IO;
using Wist.Blockchain.Core.DataModel;
using Wist.Blockchain.Core.Enums;
using Wist.Blockchain.Core.Exceptions;

namespace Wist.Blockchain.Core.Serializers.Signed
{
    public abstract class LinkedSerializerBase<T> : SignatureSupportSerializerBase<T> where T : LinkedPacketBase
    {
        public LinkedSerializerBase(PacketType packetType, ushort blockType) 
            : base(packetType, blockType)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            if (_block.HashPrev == null)
            {
                throw new PreviousHashNotProvidedException();
            }

            bw.Write(_block.HashPrev);
        }
    }
}
