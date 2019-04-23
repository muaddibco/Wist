using System;
using Wist.Blockchain.Core.DataModel.Transactional;
using Wist.Core;
using Wist.Core.Identity;

namespace Wist.Blockchain.Core.Parsers.Transactional
{
	public abstract class TransactionalTransitionalPacketParserBase : TransactionalBlockParserBase
	{
		public TransactionalTransitionalPacketParserBase(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) : base(identityKeyProvidersRegistry)
		{
		}

		protected override Memory<byte> ParseTransactional(ushort version, Memory<byte> spanBody, out TransactionalPacketBase transactionalBlockBase)
		{
			int readBytes = 0;

			byte[] destinationKey = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
			readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

			byte[] transactionPublicKey = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
			readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

			Memory<byte> spanPostBody = ParseTransactionalTransitional(version, spanBody.Slice(readBytes), out TransactionalTransitionalPacketBase transactionalTransitionalPacketBase);

			transactionalTransitionalPacketBase.DestinationKey = destinationKey;
			transactionalTransitionalPacketBase.TransactionPublicKey = transactionPublicKey;

			transactionalBlockBase = transactionalTransitionalPacketBase;

			return spanPostBody;
		}

		protected abstract Memory<byte> ParseTransactionalTransitional(ushort version, Memory<byte> spanBody, out TransactionalTransitionalPacketBase transactionalTransitionalPacketBase);
	}
}
