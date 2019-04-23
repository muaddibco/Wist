using System.IO;
using Wist.Blockchain.Core.DataModel.UtxoConfidential;
using Wist.Blockchain.Core.Enums;
using Wist.Blockchain.Core.Parsers.UtxoConfidential;
using Wist.Core.Cryptography;
using Wist.Crypto.ConfidentialAssets;
using Wist.Tests.Core;
using Xunit;

namespace Wist.Blockchain.Core.Tests.ParserTests
{
    public class UtxoConfidentialParsersTests : TestBase
    {
        [Fact]
        public void TransitionAssetTransferParserTest()
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
            ushort pubKeysCount = 10;
            byte[][] assetCommitments = new byte[pubKeysCount][];
            byte[][] pubKeys = new byte[pubKeysCount][];

            byte[] secretKey = null;
            ushort secretKeyIndex = 5;
            byte[] e = ConfidentialAssetsHelper.GetRandomSeed();
            byte[][] s = new byte[pubKeysCount][];


            ushort affiliationCommitmentsCount = 7;
            byte[][] affiliationCommitments = new byte[affiliationCommitmentsCount][];
            byte[] affiliationE = ConfidentialAssetsHelper.GetRandomSeed();
            byte[][] affiliationS = new byte[affiliationCommitmentsCount][];

            byte[] mask = ConfidentialAssetsHelper.GetRandomSeed();

            for (int i = 0; i < pubKeysCount; i++)
            {
                pubKeys[i] = BinaryHelper.GetRandomPublicKey(out byte[] secretKeyTemp);
                if(i == secretKeyIndex)
                {
                    secretKey = secretKeyTemp;
                }
                assetCommitments[i] = ConfidentialAssetsHelper.GetRandomSeed();
                s[i] = ConfidentialAssetsHelper.GetRandomSeed();
            }

            for (int i = 0; i < affiliationCommitmentsCount; i++)
            {
                affiliationCommitments[i] = ConfidentialAssetsHelper.GetRandomSeed();
                affiliationS[i] = ConfidentialAssetsHelper.GetRandomSeed();
            }

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(assetCommitment);

                    bw.Write(pubKeysCount);
                    for (int i = 0; i < pubKeysCount; i++)
                    {
                        bw.Write(assetCommitments[i]);
                    }
                    bw.Write(e);
                    for (int i = 0; i < pubKeysCount; i++)
                    {
                        bw.Write(s[i]);
                    }

                    bw.Write(affiliationCommitmentsCount);
                    for (int i = 0; i < affiliationCommitmentsCount; i++)
                    {
                        bw.Write(affiliationCommitments[i]);
                    }
                    bw.Write(affiliationE);
                    for (int i = 0; i < affiliationCommitmentsCount; i++)
                    {
                        bw.Write(affiliationS[i]);
                    }

                    bw.Write(mask);
                    bw.Write(assetCommitment);
                }

                body = ms.ToArray();
            }

            byte[] packet = BinaryHelper.GetUtxoConfidentialPacket(PacketType.UtxoConfidential, syncBlockHeight, nonce, powHash, version, 
                BlockTypes.UtxoConfidential_NonQuantitativeAssetTransfer, keyImage, destinationKey, destinationKey2, transactionPublicKey, body, pubKeys, secretKey, secretKeyIndex, 
                out RingSignature[] ringSignatures);
            TransitionAssetTransferParser parser = new TransitionAssetTransferParser(_identityKeyProvidersRegistry);
            TransitionAffiliatedAssetTransfer block = (TransitionAffiliatedAssetTransfer)parser.Parse(packet);

            Assert.Equal(syncBlockHeight, block.SyncBlockHeight);
            Assert.Equal(nonce, block.Nonce);
            Assert.Equal(powHash, block.PowHash);
            Assert.Equal(version, block.Version);
            Assert.Equal(keyImage, block.KeyImage.Value.ToArray());
            Assert.Equal(destinationKey, block.DestinationKey);
			Assert.Equal(destinationKey2, block.DestinationKey2);
			Assert.Equal(transactionPublicKey, block.TransactionPublicKey);
            Assert.Equal(assetCommitment, block.AssetCommitment);
            Assert.Equal(pubKeysCount, block.SurjectionProof.AssetCommitments.Length);

            for (int i = 0; i < pubKeysCount; i++)
            {
                Assert.Equal(assetCommitments[i], block.SurjectionProof.AssetCommitments[i]);
                Assert.Equal(s[i], block.SurjectionProof.Rs.S[i]);
                Assert.Equal(ringSignatures[i].C, block.Signatures[i].C);
                Assert.Equal(ringSignatures[i].R, block.Signatures[i].R);
            }

            Assert.Equal(e, block.SurjectionProof.Rs.E);
            Assert.Equal(mask, block.EcdhTuple.Mask);
            Assert.Equal(assetCommitment, block.EcdhTuple.AssetId);
        }

        [Fact]
        public void TransitionAssetProofParserTest()
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
                pubKeys[i] = BinaryHelper.GetRandomPublicKey(out byte[] secretKeyTemp);
                if (i == secretKeyIndex)
                {
                    secretKey = secretKeyTemp;
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

            byte[] packet = BinaryHelper.GetUtxoConfidentialPacket(PacketType.UtxoConfidential, syncBlockHeight, nonce, powHash, version,
                BlockTypes.UtxoConfidential_TransitionOnboardingDisclosingProofs, keyImage, destinationKey, destinationKey2, transactionPublicKey, body, pubKeys, secretKey, secretKeyIndex,
                out RingSignature[] ringSignatures);
            TransitionOnboardingDisclosingProofsParser parser = new TransitionOnboardingDisclosingProofsParser(_identityKeyProvidersRegistry);
            TransitionOnboardingDisclosingProofs block = (TransitionOnboardingDisclosingProofs)parser.Parse(packet);

            Assert.Equal(syncBlockHeight, block.SyncBlockHeight);
            Assert.Equal(nonce, block.Nonce);
            Assert.Equal(powHash, block.PowHash);
            Assert.Equal(version, block.Version);
            Assert.Equal(keyImage, block.KeyImage.Value.ToArray());
            Assert.Equal(destinationKey, block.DestinationKey);
            Assert.Equal(destinationKey2, block.DestinationKey2);
            Assert.Equal(transactionPublicKey, block.TransactionPublicKey);
            Assert.Equal(assetCommitment, block.AssetCommitment);
            Assert.Equal(mask, block.EcdhTuple.Mask);
            Assert.Equal(assetId, block.EcdhTuple.AssetId);
            Assert.Equal(assetIssuer, block.EcdhTuple.AssetIssuer);
            Assert.Equal(payload, block.EcdhTuple.Payload);

            Assert.Equal(pubKeysCount, block.OwnershipProof.AssetCommitments.Length);

            for (int i = 0; i < pubKeysCount; i++)
            {
                Assert.Equal(ownershipAssetCommitments[i], block.OwnershipProof.AssetCommitments[i]);
                Assert.Equal(s[i], block.OwnershipProof.Rs.S[i]);
                Assert.Equal(ringSignatures[i].C, block.Signatures[i].C);
                Assert.Equal(ringSignatures[i].R, block.Signatures[i].R);
            }

            Assert.Equal(e, block.OwnershipProof.Rs.E);

            Assert.Equal(eligibilityCommitmentsCount, block.EligibilityProof.AssetCommitments.Length);

            for (int i = 0; i < eligibilityCommitmentsCount; i++)
            {
                Assert.Equal(eligibilityCommitments[i], block.EligibilityProof.AssetCommitments[i]);
                Assert.Equal(eligibilityS[i], block.EligibilityProof.Rs.S[i]);
            }

            Assert.Equal(eligibilityE, block.EligibilityProof.Rs.E);
        }
    }
}
