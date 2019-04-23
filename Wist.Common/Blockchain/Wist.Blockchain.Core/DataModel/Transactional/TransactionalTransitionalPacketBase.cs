using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Blockchain.Core.DataModel.Transactional
{
	public abstract class TransactionalTransitionalPacketBase : TransactionalPacketBase
	{

		/// <summary>
		/// P = Hs(r * A) * G + B where A is receiver's Public View Key and B is receiver's Public Spend Key
		/// </summary>
		public byte[] DestinationKey { get; set; }

		/// <summary>
		/// R = r * G. 'r' can be erased after transaction sent unless sender wants to proof he sent funds to particular destination address.
		/// </summary>
		public byte[] TransactionPublicKey { get; set; }
	}
}
