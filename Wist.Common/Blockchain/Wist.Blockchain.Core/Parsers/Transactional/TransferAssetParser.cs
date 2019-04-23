using System;
using System.Buffers.Binary;
using Wist.Blockchain.Core.DataModel.Transactional;
using Wist.Blockchain.Core.DataModel.Transactional.Internal;
using Wist.Blockchain.Core.Enums;
using Wist.Blockchain.Core.Exceptions;
using Wist.Core;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.Identity;

namespace Wist.Blockchain.Core.Parsers.Transactional
{
	[RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
	public class TransferAssetParser : TransactionalTransitionalPacketParserBase
	{
		public TransferAssetParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) :
			base(identityKeyProvidersRegistry)
		{
		}

		public override ushort BlockType => BlockTypes.Transaction_TransferAsset;

		protected override Memory<byte> ParseTransactionalTransitional(ushort version, Memory<byte> spanBody, out TransactionalTransitionalPacketBase transactionalBlockBase)
		{
			TransferAsset block = null;

			if (version == 1)
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

				block = new TransferAsset
				{
					TransferredAsset = new EncryptedAsset
					{
						AssetCommitment = assetCommitment,
						EcdhTuple = new EcdhTupleCA
						{
							AssetId = assetId,
							Mask = mask
						}
					},
					SurjectionProof = new SurjectionProof
					{
						AssetCommitments = assetCommitments,
						Rs = new BorromeanRingSignature
						{
							E = e,
							S = s
						}
					}
				};

				transactionalBlockBase = block;
				return spanBody.Slice(readBytes);
			}

			throw new BlockVersionNotSupportedException(version, BlockType);
		}
	}
}
