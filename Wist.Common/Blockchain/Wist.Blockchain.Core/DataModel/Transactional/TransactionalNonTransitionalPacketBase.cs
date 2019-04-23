using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Blockchain.Core.DataModel.Transactional
{
	public abstract class TransactionalNonTransitionalPacketBase : TransactionalPacketBase
	{
		public byte[] Target { get; set; }
	}
}
