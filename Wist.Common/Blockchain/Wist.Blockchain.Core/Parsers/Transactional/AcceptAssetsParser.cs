using System;
using System.Buffers.Binary;
using Wist.Blockchain.Core.DataModel.Transactional;
using Wist.Blockchain.Core.DataModel.Transactional.Internal;
using Wist.Blockchain.Core.Enums;
using Wist.Blockchain.Core.Exceptions;
using Wist.Core;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Identity;

namespace Wist.Blockchain.Core.Parsers.Transactional
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
    public class AcceptAssetsParser : TransactionalBlockParserBase
    {
        public AcceptAssetsParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) 
            : base(identityKeyProvidersRegistry)
        {
        }

        public override ushort BlockType => BlockTypes.Transaction_AcceptAssets;

        protected override Memory<byte> ParseTransactional(ushort version, Memory<byte> spanBody, out TransactionalPacketBase transactionalBlockBase)
        {
            if (version == 1)
            {
                int readBytes = 0;

                ushort acceptedUnblindingAssetABsLength = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Slice(readBytes).Span);
                readBytes += sizeof(ushort);

                ushort acceptedUnblindingAssetUtxosLength = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Slice(readBytes).Span);
                readBytes += sizeof(ushort);

                AcceptedAssetUnblindingAB[] acceptedUnblindingAssetABs = new AcceptedAssetUnblindingAB[acceptedUnblindingAssetABsLength];
                AcceptedAssetUnblindingUtxo[] acceptedUnblindingAssetUtxos = new AcceptedAssetUnblindingUtxo[acceptedUnblindingAssetUtxosLength];

                for (int i = 0; i < acceptedUnblindingAssetABsLength; i++)
                {
                    byte[] assetId = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                    readBytes += Globals.NODE_PUBLIC_KEY_SIZE;
                    byte[] blindingFactor = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                    readBytes += Globals.NODE_PUBLIC_KEY_SIZE;
                    byte[] sourceAddress = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                    readBytes += Globals.NODE_PUBLIC_KEY_SIZE;
                    ulong height = BinaryPrimitives.ReadUInt64LittleEndian(spanBody.Slice(readBytes).Span);
                    readBytes += sizeof(ulong);

                    acceptedUnblindingAssetABs[i] = new AcceptedAssetUnblindingAB
                    {
                        AcceptedAssetsUnblinding = new AcceptedAssetUnblinding
                        {
                            AssetId = assetId,
                            BlindingFactor = blindingFactor
                        },
                        SourceAddress = sourceAddress,
                        SourceHeight = height
                    };
                }

                for (int i = 0; i < acceptedUnblindingAssetUtxosLength; i++)
                {
                    byte[] assetId = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                    readBytes += Globals.NODE_PUBLIC_KEY_SIZE;
                    byte[] blindingFactor = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                    readBytes += Globals.NODE_PUBLIC_KEY_SIZE;
                    byte[] sourceKey = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                    readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

                    acceptedUnblindingAssetUtxos[i] = new AcceptedAssetUnblindingUtxo
                    {
                        AcceptedAssetsUnblinding = new AcceptedAssetUnblinding
                        {
                            AssetId = assetId,
                            BlindingFactor = blindingFactor
                        },
                        SourceKey = sourceKey
                    };
                }

                ushort assetsGroupsLength = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Slice(readBytes).Span);
                readBytes += sizeof(ushort);

                AssetsGroup[] assetsGroups = new AssetsGroup[assetsGroupsLength];

                for (int i = 0; i < assetsGroupsLength; i++)
                {
                    uint groupId = BinaryPrimitives.ReadUInt32LittleEndian(spanBody.Slice(readBytes).Span);
                    readBytes += sizeof(uint);

                    ushort totalAssetIdsCount = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Slice(readBytes).Span);
                    readBytes += sizeof(ushort);

                    byte[][] assetIds = new byte[totalAssetIdsCount][];
                    ulong[] amounts = new ulong[totalAssetIdsCount];

                    for (int j = 0; j < totalAssetIdsCount; i++)
                    {
                        assetIds[j] = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                        readBytes += Globals.NODE_PUBLIC_KEY_SIZE;
                    }

                    for (int j = 0; j < totalAssetIdsCount; i++)
                    {
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


                AcceptAssets acceptAssetTransitionBlock = new AcceptAssets
                {
                    AcceptedAssetUnblindingABs = acceptedUnblindingAssetABs,
                    AcceptedAssetUnblindingUtxos = acceptedUnblindingAssetUtxos,
                    AssetsGroups = assetsGroups
                };

                transactionalBlockBase = acceptAssetTransitionBlock;

                return spanBody.Slice(readBytes);
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}
