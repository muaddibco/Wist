using System.IO;
using Wist.Blockchain.Core.DataModel.Transactional;
using Wist.Blockchain.Core.Enums;

namespace Wist.Blockchain.Core.Serializers.Signed.Transactional
{
	public abstract class TransactionalNonTransitionalSerializerBase<T> : TransactionalSerializerBase<T> where T : TransactionalNonTransitionalPacketBase
	{
		public TransactionalNonTransitionalSerializerBase(PacketType packetType, ushort blockType) : base(packetType, blockType)
		{
		}
		protected override void WriteBody(BinaryWriter bw)
		{
			base.WriteBody(bw);

			bw.Write(_block.Target);
		}
	}
}
