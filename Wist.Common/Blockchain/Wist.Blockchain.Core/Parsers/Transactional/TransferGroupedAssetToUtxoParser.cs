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
using Wist.Core.HashCalculations;
using Wist.Core.Identity;

namespace Wist.Blockchain.Core.Parsers.Transactional
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
    public class TransferGroupedAssetToUtxoParser : TransactionalTransitionalPacketParserBase
	{
        public TransferGroupedAssetToUtxoParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) 
            : base(identityKeyProvidersRegistry)
        {
        }

        public override ushort BlockType => BlockTypes.Transaction_TransferGroupedAssetsToUtxo;

        protected override Memory<byte> ParseTransactionalTransitional(ushort version, Memory<byte> spanBody, out TransactionalTransitionalPacketBase transactionalBlockBase)
        {
			TransferGroupedAssetToUtxo block = null;

            if (version == 1)
            {
                int readBytes = 0;

                byte[] assetCommitment = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

                byte[] assetId = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

                byte[] mask = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

                EncryptedAsset transferedAsset = new EncryptedAsset
                {
                    AssetCommitment = assetCommitment,
                    EcdhTuple = new EcdhTupleCA { AssetId = assetId, Mask = mask }
                };

                ushort blindedAssetsGroupsLength = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Slice(readBytes).Span);
                readBytes += sizeof(ushort);

                BlindedAssetsGroup[] blindedAssetsGroups = new BlindedAssetsGroup[blindedAssetsGroupsLength];

                ushort totalCommitments = 0;

                for (int i = 0; i < blindedAssetsGroupsLength; i++)
                {
                    uint groupId = BinaryPrimitives.ReadUInt32LittleEndian(spanBody.Slice(readBytes).Span);
                    readBytes += sizeof(uint);

                    ushort assetCommitmentsLength = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Slice(readBytes).Span);
                    readBytes += sizeof(ushort);

                    totalCommitments += assetCommitmentsLength;
                    byte[][] assetCommitments = new byte[assetCommitmentsLength][];

                    for (int j = 0; j < assetCommitmentsLength; j++)
                    {
                        assetCommitments[j] = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                        readBytes += Globals.NODE_PUBLIC_KEY_SIZE;
                    }

                    blindedAssetsGroups[i] = new BlindedAssetsGroup
                    {
                        GroupId = groupId,
                        AssetCommitments = assetCommitments
                    };
                }

                InversedSurjectionProof[] inversedSurjectionProofs = new InversedSurjectionProof[totalCommitments + 1];
                for (int i = 0; i < inversedSurjectionProofs.Length; i++)
                {
                    inversedSurjectionProofs[i] = ReadInversedSurjectionProof(spanBody.Slice(readBytes).Span, out int count);
                    readBytes += count;
                }

                block = new TransferGroupedAssetToUtxo
                {
                    TransferredAsset = transferedAsset,
                    BlindedAssetsGroups = blindedAssetsGroups,
                    InversedSurjectionProofs = inversedSurjectionProofs
                };

                transactionalBlockBase = block;
                return spanBody.Slice(readBytes);
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}
