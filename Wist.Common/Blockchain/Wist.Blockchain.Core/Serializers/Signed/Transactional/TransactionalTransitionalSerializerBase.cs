using System.IO;
using Wist.Blockchain.Core.DataModel.Transactional;
using Wist.Blockchain.Core.Enums;

namespace Wist.Blockchain.Core.Serializers.Signed.Transactional
{
	public abstract class TransactionalTransitionalSerializerBase<T> : TransactionalSerializerBase<T> where T : TransactionalTransitionalPacketBase
	{
		public TransactionalTransitionalSerializerBase(PacketType packetType, ushort blockType) : base(packetType, blockType)
		{
		}
		protected override void WriteBody(BinaryWriter bw)
		{
			base.WriteBody(bw);

			bw.Write(_block.DestinationKey);
			bw.Write(_block.TransactionPublicKey);
		}
	}
}
