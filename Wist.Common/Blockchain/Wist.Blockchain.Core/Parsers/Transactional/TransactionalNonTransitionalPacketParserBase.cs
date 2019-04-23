using System;
using Wist.Blockchain.Core.DataModel.Transactional;
using Wist.Core;
using Wist.Core.Identity;

namespace Wist.Blockchain.Core.Parsers.Transactional
{
	public abstract class TransactionalNonTransitionalPacketParserBase : TransactionalBlockParserBase
	{
		public TransactionalNonTransitionalPacketParserBase(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) : base(identityKeyProvidersRegistry)
		{
		}

		protected override Memory<byte> ParseTransactional(ushort version, Memory<byte> spanBody, out TransactionalPacketBase transactionalBlockBase)
		{
			int readBytes = 0;

			byte[] target = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
			readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

			Memory<byte> spanPostBody = ParseTransactionalNonTransitional(version, spanBody.Slice(readBytes), out TransactionalNonTransitionalPacketBase transactionalNonTransitionalPacketBase);

			transactionalNonTransitionalPacketBase.Target = target;

			transactionalBlockBase = transactionalNonTransitionalPacketBase;

			return spanPostBody;
		}

		protected abstract Memory<byte> ParseTransactionalNonTransitional(ushort version, Memory<byte> spanBody, out TransactionalNonTransitionalPacketBase transactionalTransitionalPacketBase);
	}
}
