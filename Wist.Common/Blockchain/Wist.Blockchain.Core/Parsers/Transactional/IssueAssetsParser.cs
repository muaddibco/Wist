using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;
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
    public class IssueAssetsParser : TransactionalBlockParserBase
    {
        public IssueAssetsParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) 
            : base(identityKeyProvidersRegistry)
        {
        }

        public override ushort BlockType => BlockTypes.Transaction_IssueGroupedAssets;

        protected override Memory<byte> ParseTransactional(ushort version, Memory<byte> spanBody, out TransactionalPacketBase transactionalBlockBase)
        {
            IssueGroupedAssets block = null;

            if(version == 1)
            {
                int readBytes = 0;

                ushort assetsIssuanceGroupsLength = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Slice(readBytes).Span);
                readBytes += sizeof(ushort);

                ushort blindedAssetsIssuanceGroupsLength = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Slice(readBytes).Span);
                readBytes += sizeof(ushort);

                AssetsIssuanceGroup[] assetsIssuanceGroups = new AssetsIssuanceGroup[assetsIssuanceGroupsLength];
                BlindedAssetsIssuanceGroup[] blindedAssetsIssuanceGroups = new BlindedAssetsIssuanceGroup[blindedAssetsIssuanceGroupsLength];

                for (int i = 0; i < assetsIssuanceGroupsLength; i++)
                {
                    uint groupId = BinaryPrimitives.ReadUInt32LittleEndian(spanBody.Slice(readBytes).Span);
                    readBytes += sizeof(uint);

                    ushort assetIdsLength = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Slice(readBytes).Span);
                    readBytes += sizeof(ushort);

                    AssetIssuance[] assetIssuances = new AssetIssuance[assetIdsLength];

                    for (int j = 0; j < assetIdsLength; j++)
                    {
                        byte[] assetId = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                        readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

                        byte strLen = spanBody.Slice(readBytes, 1).ToArray()[0];
                        readBytes++;

                        string issuedAssetInfo = Encoding.ASCII.GetString(spanBody.Slice(readBytes, strLen).ToArray());
                        readBytes += strLen;

                        assetIssuances[j] = new AssetIssuance
                        {
                            AssetId = assetId,
                            IssuedAssetInfo = issuedAssetInfo
                        };
                    }

                    assetsIssuanceGroups[i] = new AssetsIssuanceGroup
                    {
                        GroupId = groupId,
                        AssetIssuances = assetIssuances
                    };
                }

                for (int i = 0; i < blindedAssetsIssuanceGroupsLength; i++)
                {
                    uint groupId = BinaryPrimitives.ReadUInt32LittleEndian(spanBody.Slice(readBytes).Span);
                    readBytes += sizeof(uint);

                    ushort assetCommitmentsLength = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Slice(readBytes).Span);
                    readBytes += sizeof(ushort);

                    byte[][] assetCommitments = new byte[assetCommitmentsLength][];

                    for (int j = 0; j < assetCommitmentsLength; j++)
                    {
                        assetCommitments[j] = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                        readBytes += Globals.NODE_PUBLIC_KEY_SIZE;
                    }

                    AssetIssuance[] blindedAssetIssuances = new AssetIssuance[assetCommitmentsLength];
                    for (int j = 0; j < assetCommitmentsLength; j++)
                    {
                        byte[] assetId = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                        readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

                        byte strLen = spanBody.Slice(readBytes, 1).ToArray()[0];
                        readBytes++;

                        string issuedAssetInfo = Encoding.ASCII.GetString(spanBody.Slice(readBytes, strLen).ToArray());
                        readBytes += strLen;

                        blindedAssetIssuances[i] = new AssetIssuance
                        {
                            AssetId = assetId,
                            IssuedAssetInfo = issuedAssetInfo
                        };
                    }

                    IssuanceProof[] issuanceProofs = new IssuanceProof[assetCommitmentsLength];
                    for (int j = 0; j < assetCommitmentsLength; j++)
                    {
                        SurjectionProof surjectionProof = ReadSurjectionProof(spanBody.Slice(readBytes).Span, out int count);
                        readBytes += count;

                        byte[] mask = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                        readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

                        issuanceProofs[j] = new IssuanceProof
                        {
                            SurjectionProof = surjectionProof,
                            Mask = mask,
                        };
                    }

                    blindedAssetsIssuanceGroups[i] = new BlindedAssetsIssuanceGroup
                    {
                        GroupId = groupId,
                        AssetCommitments = assetCommitments,
                        AssetIssuances = blindedAssetIssuances,
                        IssuanceProofs = issuanceProofs
                    };
                }

                byte strLen2 = spanBody.Slice(readBytes, 1).ToArray()[0];
                readBytes++;

                string issuanceInfo = Encoding.ASCII.GetString(spanBody.Slice(readBytes, strLen2).ToArray());
                readBytes += strLen2;

                ushort assetsGroupsLength = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Slice(readBytes).Span);
                readBytes += sizeof(ushort);

                ushort blindedAssetGroupsLength = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Slice(readBytes).Span);
                readBytes += sizeof(ushort);

                AssetsGroup[] assetsGroups = new AssetsGroup[assetsGroupsLength];
                BlindedAssetsGroup[] blindedAssetsGroups = new BlindedAssetsGroup[blindedAssetGroupsLength];

                for (int i = 0; i < assetsGroupsLength; i++)
                {
                    uint groupId = BinaryPrimitives.ReadUInt32LittleEndian(spanBody.Slice(readBytes).Span);
                    readBytes += sizeof(uint);

                    ushort assetIdsLength = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Slice(readBytes).Span);
                    readBytes += sizeof(ushort);

                    byte[][] assetIds = new byte[assetIdsLength][];
                    ulong[] amounts = new ulong[assetIdsLength];

                    for (int j = 0; j < assetIdsLength; j++)
                    {
                        assetIds[j] = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                        readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

                        amounts[j] = BinaryPrimitives.ReadUInt64LittleEndian(spanBody.Slice(readBytes).Span);
                        readBytes += sizeof(ulong);
                    }

                    assetsGroups[i] = new AssetsGroup
                    {
                        GroupId = groupId,
                        AssetIds = assetIds,
                        AssetAmounts = amounts
                    };
                }

                for (int i = 0; i < blindedAssetGroupsLength; i++)
                {
                    uint groupId = BinaryPrimitives.ReadUInt32LittleEndian(spanBody.Slice(readBytes).Span);
                    readBytes += sizeof(uint);

                    ushort assetCommitmentsLength = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Slice(readBytes).Span);
                    readBytes += sizeof(ushort);

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

                block = new IssueGroupedAssets
                {
                    AssetsIssuanceGroups = assetsIssuanceGroups,
                    BlindedAssetsIssuanceGroups = blindedAssetsIssuanceGroups,
                    IssuanceInfo = issuanceInfo,
                    AssetsGroups = assetsGroups,
                    BlindedAssetsGroups = blindedAssetsGroups
                };

                transactionalBlockBase = block;
                return spanBody.Slice(readBytes);
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}
