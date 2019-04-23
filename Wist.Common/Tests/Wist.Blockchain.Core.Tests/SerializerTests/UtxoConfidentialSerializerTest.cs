using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wist.Blockchain.Core.DataModel.UtxoConfidential;
using Wist.Blockchain.Core.Enums;
using Wist.Blockchain.Core.Parsers.UtxoConfidential;
using Wist.Blockchain.Core.Serializers.UtxoConfidential;
using Wist.Core.Cryptography;
using Wist.Core.Identity;
using Wist.Crypto.ConfidentialAssets;
using Wist.Core.ExtensionMethods;
using Wist.Tests.Core;
using Xunit;

namespace Wist.Blockchain.Core.Tests.SerializerTests
{
    public class UtxoConfidentialSerializerTests : TestBase
    {
        [Fact]
        public void TransitionAssetProofSerializerTest()
        {
            ulong syncBlockHeight = 1;
            uint nonce = 4;
            byte[] powHash = BinaryHelper.GetPowHash(1234);
            ushort version = 1;
            byte[] body;
            byte[] transactionPublicKey = ConfidentialAssetsHelper.GetRandomSeed();
            byte[] destinationKey = ConfidentialAssetsHelper.GetRandomSeed();
            byte[] destinationKey2 = ConfidentialAssetsHelper.GetRandomSeed();
            byte[] keyImage = BinaryHelper.GetRandomPublicKey();
            byte[] assetCommitment = ConfidentialAssetsHelper.GetRandomSeed();

            byte[] mask = ConfidentialAssetsHelper.GetRandomSeed();
            byte[] assetId = ConfidentialAssetsHelper.GetRandomSeed();
            byte[] assetIssuer = ConfidentialAssetsHelper.GetRandomSeed();
            byte[] payload = ConfidentialAssetsHelper.GetRandomSeed();

            ushort pubKeysCount = 10;
            byte[][] ownershipAssetCommitments = new byte[pubKeysCount][];
            byte[][] pubKeys = new byte[pubKeysCount][];

            byte[] secretKey = null;
            ushort secretKeyIndex = 5;
            byte[] e = ConfidentialAssetsHelper.GetRandomSeed();
            byte[][] s = new byte[pubKeysCount][];


            ushort eligibilityCommitmentsCount = 7;
            byte[][] eligibilityCommitments = new byte[eligibilityCommitmentsCount][];
            byte[] eligibilityE = ConfidentialAssetsHelper.GetRandomSeed();
            byte[][] eligibilityS = new byte[eligibilityCommitmentsCount][];


            for (int i = 0; i < pubKeysCount; i++)
            {
                if (i == secretKeyIndex)
                {
                    secretKey = ConfidentialAssetsHelper.GetOTSK(transactionPublicKey, _privateViewKey, _privateKey);
                    pubKeys[i] = ConfidentialAssetsHelper.GetPublicKey(secretKey);
                    keyImage = ConfidentialAssetsHelper.GenerateKeyImage(secretKey);
                }
                else
                {
                    pubKeys[i] = BinaryHelper.GetRandomPublicKey(out byte[] secretKeyTemp);
                }

                ownershipAssetCommitments[i] = ConfidentialAssetsHelper.GetRandomSeed();
                s[i] = ConfidentialAssetsHelper.GetRandomSeed();
            }

            for (int i = 0; i < eligibilityCommitmentsCount; i++)
            {
                eligibilityCommitments[i] = ConfidentialAssetsHelper.GetRandomSeed();
                eligibilityS[i] = ConfidentialAssetsHelper.GetRandomSeed();
            }

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(assetCommitment);

                    bw.Write(mask);
                    bw.Write(assetId);
                    bw.Write(assetIssuer);
                    bw.Write(payload);

                    bw.Write(pubKeysCount);
                    for (int i = 0; i < pubKeysCount; i++)
                    {
                        bw.Write(ownershipAssetCommitments[i]);
                    }
                    bw.Write(e);
                    for (int i = 0; i < pubKeysCount; i++)
                    {
                        bw.Write(s[i]);
                    }

                    bw.Write(eligibilityCommitmentsCount);
                    for (int i = 0; i < eligibilityCommitmentsCount; i++)
                    {
                        bw.Write(eligibilityCommitments[i]);
                    }
                    bw.Write(eligibilityE);
                    for (int i = 0; i < eligibilityCommitmentsCount; i++)
                    {
                        bw.Write(eligibilityS[i]);
                    }
                }

                body = ms.ToArray();
            }

            byte[] expectedPacket = BinaryHelper.GetUtxoConfidentialPacket(PacketType.UtxoConfidential, syncBlockHeight, nonce, powHash, version,
                BlockTypes.UtxoConfidential_TransitionOnboardingDisclosingProofs, keyImage, destinationKey, destinationKey2, transactionPublicKey, body, pubKeys, secretKey, secretKeyIndex,
                out RingSignature[] ringSignatures);

            TransitionOnboardingDisclosingProofs block = new TransitionOnboardingDisclosingProofs
            {
                SyncBlockHeight = syncBlockHeight,
                Nonce = nonce,
                PowHash = powHash,
                DestinationKey = destinationKey,
				DestinationKey2 = destinationKey2,
                KeyImage = new Key32(keyImage),
                TransactionPublicKey = transactionPublicKey,
                AssetCommitment = assetCommitment,
                EcdhTuple = new EcdhTupleProofs
                {
                    Mask = mask,
                    AssetId = assetId,
                    AssetIssuer = assetIssuer,
                    Payload = payload
                },
                OwnershipProof = new SurjectionProof
                {
                    AssetCommitments = ownershipAssetCommitments,
                    Rs = new BorromeanRingSignature
                    {
                        E = e,
                        S = s
                    }
                },
                EligibilityProof = new SurjectionProof
                {
                    AssetCommitments = eligibilityCommitments,
                    Rs = new BorromeanRingSignature
                    {
                        E = eligibilityE,
                        S = eligibilityS
                    }
                }
            };

            TransitionOnboardingDisclosingProofsSerializer serializer = new TransitionOnboardingDisclosingProofsSerializer();
            serializer.Initialize(block);
            serializer.SerializeBody();
            _utxoSigningService.Sign(block, new UtxoSignatureInput(transactionPublicKey, pubKeys, secretKeyIndex));

            byte[] actualPacket = serializer.GetBytes();

            Span<byte> expectedSpan = new Span<byte>(expectedPacket);
            Span<byte> actualSpan = new Span<byte>(actualPacket);

            Assert.Equal(expectedSpan.Slice(0, expectedPacket.Length - pubKeysCount * 64).ToArray(), actualSpan.Slice(0, actualPacket.Length - pubKeysCount * 64).ToArray());
        }
    }
}
