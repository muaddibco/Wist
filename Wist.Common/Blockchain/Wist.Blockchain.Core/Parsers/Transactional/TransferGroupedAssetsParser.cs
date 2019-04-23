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
    public class TransferGroupedAssetsParser : TransactionalBlockParserBase
    {
        public TransferGroupedAssetsParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) 
                : base(identityKeyProvidersRegistry)
        {
        }

        public override ushort BlockType => BlockTypes.Transaction_IssueGroupedAssets;

        protected override Memory<byte> ParseTransactional(ushort version, Memory<byte> spanBody, out TransactionalPacketBase transactionalBlockBase)
        {
            TransferGroupedAssets block = null;

            if (version == 1)
            {
                int readBytes = 0;

                byte[] target = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

                ushort transferredAssetsLength = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Slice(readBytes).Span);
                readBytes += sizeof(ushort);

                EncryptedAsset[] transferedAssets = new EncryptedAsset[transferredAssetsLength];

                for (int i = 0; i < transferredAssetsLength; i++)
                {
                    byte[] assetCommitment = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                    readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

                    byte[] assetId = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                    readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

                    byte[] mask = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                    readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

                    transferedAssets[i] = new EncryptedAsset
                    {
                        AssetCommitment = assetCommitment,
                        EcdhTuple = new EcdhTupleCA { AssetId = assetId, Mask = mask }
                    };
                }

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

                    for (int j = 0; j < assetCommitmentsLength; i++)
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

                InversedSurjectionProof[] inversedSurjectionProofs = new InversedSurjectionProof[transferredAssetsLength + totalCommitments];
                for (int i = 0; i < inversedSurjectionProofs.Length; i++)
                {
                    inversedSurjectionProofs[i] = ReadInversedSurjectionProof(spanBody.Slice(readBytes).Span, out int count);
                    readBytes += count;
                }

                block = new TransferGroupedAssets
                {
                    TransferedAssets = transferedAssets,
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
