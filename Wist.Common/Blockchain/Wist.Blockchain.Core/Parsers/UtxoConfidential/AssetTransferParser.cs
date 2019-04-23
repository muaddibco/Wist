using System;
using System.Buffers.Binary;
using Wist.Blockchain.Core.DataModel.UtxoConfidential;
using Wist.Blockchain.Core.Enums;
using Wist.Blockchain.Core.Exceptions;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.Identity;

namespace Wist.Blockchain.Core.Parsers.UtxoConfidential
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
    public class AssetTransferParser : UtxoConfidentialParserBase
    {
        public AssetTransferParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) : base(identityKeyProvidersRegistry)
        {
        }

        public override ushort BlockType => BlockTypes.UtxoConfidential_NonQuantitativeAssetTransfer;

        protected override Memory<byte> ParseUtxoConfidential(ushort version, Memory<byte> spanBody, out UtxoConfidentialBase utxoConfidentialBase)
        {
            UtxoConfidentialBase block = null;

            if (version == 1)
            {
                int readBytes = 0;
                byte[] assetCommitment = spanBody.Slice(readBytes, 32).ToArray();
                readBytes += 32;

                ushort assetCommitmentsCount = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Slice(readBytes).Span);
                readBytes += 2;

                byte[][] assetCommitments = new byte[assetCommitmentsCount][];
                for (int i = 0; i < assetCommitmentsCount; i++)
                {
                    assetCommitments[i] = spanBody.Slice(readBytes, 32).ToArray();
                    readBytes += 32;
                }

                byte[] e = spanBody.Slice(readBytes, 32).ToArray();
                readBytes += 32;

                byte[][] s = new byte[assetCommitmentsCount][];
                for (int i = 0; i < assetCommitmentsCount; i++)
                {
                    s[i] = spanBody.Slice(readBytes, 32).ToArray();
                    readBytes += 32;
                }

                byte[] mask = spanBody.Slice(readBytes, 32).ToArray();
                readBytes += 32;

                byte[] assetId = spanBody.Slice(readBytes, 32).ToArray();
                readBytes += 32;

                SurjectionProof surjectionProof = new SurjectionProof
                {
                    AssetCommitments = assetCommitments,
                    Rs = new BorromeanRingSignature
                    {
                        E = e,
                        S = s
                    }
                };

                block = new AssetTransfer
                {
                    AssetCommitment = assetCommitment,
                    SurjectionProof = surjectionProof,
                    EcdhTuple = new EcdhTupleCA
                    {
                        Mask = mask,
                        AssetId = assetId
                    }
                };

                utxoConfidentialBase = block;

                return spanBody.Slice(readBytes);
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}
