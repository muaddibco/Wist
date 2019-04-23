using System;
using System.Buffers.Binary;
using Wist.Blockchain.Core.DataModel.Transactional;
using Wist.Blockchain.Core.Enums;
using Wist.Blockchain.Core.Exceptions;
using Wist.Core;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Identity;

namespace Wist.Blockchain.Core.Parsers.Transactional
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
	public class RetransferAssetToUtxoParser : TransactionalTransitionalPacketParserBase
	{
        public RetransferAssetToUtxoParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) : 
            base(identityKeyProvidersRegistry)
        {
        }

        public override ushort BlockType => BlockTypes.Transaction_RetransferAssetToUtxo;

        protected override Memory<byte> ParseTransactionalTransitional(ushort version, Memory<byte> spanBody, out TransactionalTransitionalPacketBase transactionalBlockBase)
        {
            RetransferAssetToUtxo block = null;

            if(version == 1)
            {
                int readBytes = 0;

                byte[] assetCommitment = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

                byte[] assetId = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

                byte[] mask = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

                ushort assetCommitmentsCount = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Slice(readBytes).Span);
                readBytes += sizeof(ushort);

                byte[][] assetCommitments = new byte[assetCommitmentsCount][];

                for (int i = 0; i < assetCommitmentsCount; i++)
                {
                    assetCommitments[i] = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                    readBytes += Globals.NODE_PUBLIC_KEY_SIZE;
                }

                byte[] e = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

                byte[][] s = new byte[assetCommitmentsCount][];

                for (int i = 0; i < assetCommitmentsCount; i++)
                {
                    s[i] = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                    readBytes += Globals.NODE_PUBLIC_KEY_SIZE;
                }

                transactionalBlockBase = block;
                return spanBody.Slice(readBytes);
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}
