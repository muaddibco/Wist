using System;
using System.IO;
using System.Text;
using Wist.Blockchain.Core.DataModel.Transactional;
using Wist.Blockchain.Core.Enums;
using Wist.Blockchain.Core.Parsers.Transactional;
using Wist.Core.Cryptography;
using Wist.Crypto.ConfidentialAssets;
using Wist.Tests.Core;
using Xunit;

namespace Wist.Blockchain.Core.Tests.ParserTests
{
	public class TransactionalParsersTests : TestBase
	{
		[Fact]
		public void TransferFundsBlockParserTest()
		{
			ulong tagId = 147;
			ulong syncBlockHeight = 1;
			uint nonce = 4;
			byte[] powHash = BinaryHelper.GetPowHash(1234);
			ushort version = 1;
			ulong blockHeight = 9;
			byte[] body;

			ulong uptodateFunds = 10001;
			byte[] targetOriginalHash = BinaryHelper.GetDefaultHash(112233);

			using (MemoryStream ms = new MemoryStream())
			{
				using (BinaryWriter bw = new BinaryWriter(ms))
				{
					bw.Write(tagId);
					bw.Write(uptodateFunds);
					bw.Write(targetOriginalHash);
				}

				body = ms.ToArray();
			}

			byte[] packet = BinaryHelper.GetSignedPacket(PacketType.Transactional, syncBlockHeight, nonce, powHash, version,
				BlockTypes.Transaction_TransferFunds, blockHeight, null, body, _privateKey, out byte[] expectedSignature);

			TransferFundsBlockParser parser = new TransferFundsBlockParser(_identityKeyProvidersRegistry);
			TransferFundsBlock block = (TransferFundsBlock)parser.Parse(packet);

			Assert.Equal(syncBlockHeight, block.SyncBlockHeight);
			Assert.Equal(nonce, block.Nonce);
			Assert.Equal(powHash, block.PowHash);
			Assert.Equal(version, block.Version);
			Assert.Equal(blockHeight, block.BlockHeight);
			Assert.Equal(uptodateFunds, block.UptodateFunds);
			Assert.Equal(targetOriginalHash, block.TargetOriginalHash);

			Assert.Equal(_publicKey, block.Signer.Value.ToArray());
			Assert.Equal(expectedSignature, block.Signature.ToArray());
		}

		[Fact]
		public void IssueAssetsBlockParserTest()
		{
			ulong tagId = 147;
			ulong syncBlockHeight = 1;
			uint nonce = 4;
			byte[] powHash = BinaryHelper.GetPowHash(1234);
			ushort version = 1;
			ulong blockHeight = 9;
			byte[] body;

			ulong uptodateFunds = 10001;

			ushort ownedNonBlindedAssetGroupsCount = 3, ownedBlindedAssetGroupsCount = 3;
			ushort ownedNonBlindedAssetIdsCount = 5, ownedAssetCommitmentsCount = 5;
			uint[] nonBlindedGroupIds = new uint[ownedNonBlindedAssetGroupsCount];
			byte[][][] ownedAssetIds = new byte[ownedNonBlindedAssetGroupsCount][][];
			ulong[][] ownedAssetAmounts = new ulong[ownedNonBlindedAssetGroupsCount][];
			uint[] blindedGroupIds = new uint[ownedBlindedAssetGroupsCount];
			byte[][][] ownedAssetCommitments = new byte[ownedBlindedAssetGroupsCount][][];

			ushort issuanceNonBlindedGroupsCount = 2, issuanceBlindedGroupsCount = 2;
			ushort issuedNonBlindedAssetsCount = 10, issuedBlindedAssetsCount = 10;
			uint[] issuanceNonBlindedGroupIds = new uint[issuanceNonBlindedGroupsCount], issuanceBlindedGroupIds = new uint[issuanceBlindedGroupsCount];
			byte[][][] issuedNonBlindedAssetIds = new byte[issuanceNonBlindedGroupsCount][][];
			string[][] issuedNonBlindedAssetInfos = new string[issuanceNonBlindedGroupsCount][];

			byte[][][] issuedAssetCommitments = new byte[issuanceBlindedGroupsCount][][];
			byte[][][] issuedBlindedAssetIds = new byte[issuanceNonBlindedGroupsCount][][];
			string[][] issuedBlindedAssetInfos = new string[issuanceNonBlindedGroupsCount][];

			byte[][][] surjectionProofCommitments = new byte[issuanceBlindedGroupsCount][][];
			byte[][][] surjectionProofE = new byte[issuanceBlindedGroupsCount][][];
			byte[][][] surjectionProofS = new byte[issuanceBlindedGroupsCount][][];
			byte[][][] issuanceProofMask = new byte[issuanceBlindedGroupsCount][][];

			for (int i = 0; i < issuanceNonBlindedGroupsCount; i++)
			{
				issuanceNonBlindedGroupIds[i] = (uint)i;
				issuedNonBlindedAssetIds[i] = new byte[issuedNonBlindedAssetsCount][];
				issuedNonBlindedAssetInfos[i] = new string[issuedNonBlindedAssetsCount];

				for (int j = 0; j < issuedNonBlindedAssetsCount; j++)
				{
					issuedNonBlindedAssetIds[i][j] = ConfidentialAssetsHelper.GetRandomSeed();
					issuedNonBlindedAssetInfos[i][j] = $"Issued Asset #{i}_{j}";
				}
			}

			for (int i = 0; i < issuanceBlindedGroupsCount; i++)
			{
				issuanceBlindedGroupIds[i] = (uint)i + 10;
				issuedAssetCommitments[i] = new byte[issuedBlindedAssetsCount][];
				issuedBlindedAssetIds[i] = new byte[issuedBlindedAssetsCount][];
				issuedBlindedAssetInfos[i] = new string[issuedBlindedAssetsCount];
				surjectionProofCommitments[i] = new byte[issuedBlindedAssetsCount][];
				surjectionProofE[i] = new byte[issuedBlindedAssetsCount][];
				surjectionProofS[i] = new byte[issuedBlindedAssetsCount][];
				issuanceProofMask[i] = new byte[issuedBlindedAssetsCount][];

				for (int j = 0; j < issuedBlindedAssetsCount; j++)
				{
					issuedAssetCommitments[i][j] = ConfidentialAssetsHelper.GetRandomSeed();
					issuedBlindedAssetIds[i][j] = ConfidentialAssetsHelper.GetRandomSeed();
					issuedBlindedAssetInfos[i][j] = $"Issued Blinded Asset #{i}_{j}";
					surjectionProofCommitments[i][j] = ConfidentialAssetsHelper.GetRandomSeed();
					surjectionProofE[i][j] = ConfidentialAssetsHelper.GetRandomSeed();
					surjectionProofS[i][j] = ConfidentialAssetsHelper.GetRandomSeed();
					issuanceProofMask[i][j] = ConfidentialAssetsHelper.GetRandomSeed();
				}
			}

			string issuanceInfo = "Issuance Info";

			Random random = new Random();

			for (int i = 0; i < ownedNonBlindedAssetGroupsCount; i++)
			{
				nonBlindedGroupIds[i] = (uint)i;
				ownedAssetIds[i] = new byte[ownedNonBlindedAssetIdsCount][];
				ownedAssetAmounts[i] = new ulong[ownedNonBlindedAssetIdsCount];

				for (int j = 0; j < ownedNonBlindedAssetIdsCount; j++)
				{
					ownedAssetIds[i][j] = ConfidentialAssetsHelper.GetRandomSeed();
					ownedAssetAmounts[i][j] = (ulong)random.Next();
				}
			}

			for (int i = 0; i < ownedBlindedAssetGroupsCount; i++)
			{
				blindedGroupIds[i] = (uint)i + 10;
				ownedAssetCommitments[i] = new byte[ownedAssetCommitmentsCount][];

				for (int j = 0; j < ownedAssetCommitmentsCount; j++)
				{
					ownedAssetCommitments[i][j] = ConfidentialAssetsHelper.GetRandomSeed();
				}
			}

			using (MemoryStream ms = new MemoryStream())
			{
				using (BinaryWriter bw = new BinaryWriter(ms))
				{
					bw.Write(tagId);
					bw.Write(uptodateFunds);

					bw.Write(issuanceNonBlindedGroupsCount);
					bw.Write(issuanceBlindedGroupsCount);

					for (int i = 0; i < issuanceNonBlindedGroupsCount; i++)
					{
						bw.Write(issuanceNonBlindedGroupIds[i]);
						bw.Write(issuedNonBlindedAssetsCount);

						for (int j = 0; j < issuedNonBlindedAssetsCount; j++)
						{
							bw.Write(issuedNonBlindedAssetIds[i][j]);
							byte strLen = (byte)issuedNonBlindedAssetInfos[i][j].Length;
							bw.Write(strLen);
							bw.Write(Encoding.ASCII.GetBytes(issuedNonBlindedAssetInfos[i][j]));
						}
					}

					for (int i = 0; i < issuanceBlindedGroupsCount; i++)
					{
						bw.Write(issuanceBlindedGroupIds[i]);
						bw.Write(issuedBlindedAssetsCount);

						for (int j = 0; j < issuedBlindedAssetsCount; j++)
						{
							bw.Write(issuedAssetCommitments[i][j]);
						}

						for (int j = 0; j < issuedBlindedAssetsCount; j++)
						{
							bw.Write(issuedBlindedAssetIds[i][j]);
							byte strLen = (byte)issuedBlindedAssetInfos[i][j].Length;
							bw.Write(strLen);
							bw.Write(Encoding.ASCII.GetBytes(issuedBlindedAssetInfos[i][j]));
						}

						for (int j = 0; j < issuedBlindedAssetsCount; j++)
						{
							bw.Write((ushort)1);
							bw.Write(surjectionProofCommitments[i][j]);
							bw.Write(surjectionProofE[i][j]);
							bw.Write(surjectionProofS[i][j]);
							bw.Write(issuanceProofMask[i][j]);
						}
					}

					bw.Write((byte)issuanceInfo.Length);
					bw.Write(Encoding.ASCII.GetBytes(issuanceInfo));

					bw.Write(ownedNonBlindedAssetGroupsCount);
					bw.Write(ownedBlindedAssetGroupsCount);

					for (int i = 0; i < ownedNonBlindedAssetGroupsCount; i++)
					{
						bw.Write(nonBlindedGroupIds[i]);
						bw.Write(ownedNonBlindedAssetIdsCount);

						for (int j = 0; j < ownedNonBlindedAssetIdsCount; j++)
						{
							bw.Write(ownedAssetIds[i][j]);
						}

						for (int j = 0; j < ownedNonBlindedAssetIdsCount; j++)
						{
							bw.Write(ownedAssetAmounts[i][j]);
						}
					}

					for (int i = 0; i < ownedBlindedAssetGroupsCount; i++)
					{
						bw.Write(blindedGroupIds[i]);
						bw.Write(ownedAssetCommitmentsCount);

						for (int j = 0; j < ownedAssetCommitmentsCount; j++)
						{
							bw.Write(ownedAssetCommitments[i][j]);
						}
					}
				}

				body = ms.ToArray();
			}

			byte[] packet = BinaryHelper.GetSignedPacket(PacketType.Transactional, syncBlockHeight, nonce, powHash, version,
				BlockTypes.Transaction_IssueGroupedAssets, blockHeight, null, body, _privateKey, out byte[] expectedSignature);

			IssueAssetsParser parser = new IssueAssetsParser(_identityKeyProvidersRegistry);
			IssueGroupedAssets block = (IssueGroupedAssets)parser.Parse(packet);

			Assert.Equal(syncBlockHeight, block.SyncBlockHeight);
			Assert.Equal(nonce, block.Nonce);
			Assert.Equal(powHash, block.PowHash);
			Assert.Equal(version, block.Version);
			Assert.Equal(blockHeight, block.BlockHeight);
			Assert.Equal(uptodateFunds, block.UptodateFunds);

			Assert.Equal(_publicKey, block.Signer.Value.ToArray());
			Assert.Equal(expectedSignature, block.Signature.ToArray());
		}

		[Fact]
		public void TransferGroupedAssetsToUtxoParserTest()
		{
			ulong tagId = 147;
			ulong syncBlockHeight = 1;
			uint nonce = 4;
			byte[] powHash = BinaryHelper.GetPowHash(1234);
			ushort version = 1;
			ulong blockHeight = 9;
			byte[] body;

			ulong uptodateFunds = 10001;
			byte[] transactionPublicKey = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] destinationKey = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] assetId = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] assetCommitment = ConfidentialAssetsHelper.GetRandomSeed();
			ushort blindedAssetsGroupsCount = 2;
			ushort assetsCount = 10;
			uint[] groupIds = new uint[blindedAssetsGroupsCount];
			byte[][][] ownedAssetCommitments = new byte[blindedAssetsGroupsCount][][];
			byte[][] surjectionAssetCommitment = new byte[blindedAssetsGroupsCount * assetsCount + 1][];
			byte[][] e = new byte[blindedAssetsGroupsCount * assetsCount + 1][];
			byte[][][] s = new byte[blindedAssetsGroupsCount * assetsCount + 1][][];
			byte[] mask = ConfidentialAssetsHelper.GetRandomSeed();

			Random random = new Random();

			for (int i = 0; i < blindedAssetsGroupsCount; i++)
			{
				groupIds[i] = (uint)i;
				ownedAssetCommitments[i] = new byte[assetsCount][];
				for (int j = 0; j < assetsCount; j++)
				{
					ownedAssetCommitments[i][j] = ConfidentialAssetsHelper.GetRandomSeed();
				}
			}

			for (int i = 0; i < blindedAssetsGroupsCount * assetsCount + 1; i++)
			{
				surjectionAssetCommitment[i] = ConfidentialAssetsHelper.GetRandomSeed();
				e[i] = ConfidentialAssetsHelper.GetRandomSeed();
				s[i] = new byte[blindedAssetsGroupsCount * assetsCount + 1][];
				for (int j = 0; j < blindedAssetsGroupsCount * assetsCount + 1; j++)
				{
					s[i][j] = ConfidentialAssetsHelper.GetRandomSeed();
				}
			}

			using (MemoryStream ms = new MemoryStream())
			{
				using (BinaryWriter bw = new BinaryWriter(ms))
				{
					bw.Write(tagId);
					bw.Write(uptodateFunds);

					bw.Write(destinationKey);
					bw.Write(transactionPublicKey);

					// TransferredAsset
					// ==============================
					bw.Write(assetCommitment);
					bw.Write(assetId);
					bw.Write(mask);
					// ==============================

					bw.Write(blindedAssetsGroupsCount);
					for (int i = 0; i < blindedAssetsGroupsCount; i++)
					{
						bw.Write(groupIds[i]);
						bw.Write(assetsCount);
						for (int j = 0; j < assetsCount; j++)
						{
							bw.Write(ownedAssetCommitments[i][j]);
						}
					}

					for (int i = 0; i < blindedAssetsGroupsCount * assetsCount + 1; i++)
					{
						bw.Write(surjectionAssetCommitment[i]);
						bw.Write(e[i]);
						bw.Write((ushort)(blindedAssetsGroupsCount * assetsCount + 1));

						for (int j = 0; j < blindedAssetsGroupsCount * assetsCount + 1; j++)
						{
							bw.Write(s[i][j]);
						}
					}
				}

				body = ms.ToArray();
			}

			byte[] packet = BinaryHelper.GetSignedPacket(PacketType.Transactional, syncBlockHeight, nonce, powHash, version,
				BlockTypes.Transaction_TransferGroupedAssetsToUtxo, blockHeight, null, body, _privateKey, out byte[] expectedSignature);

			TransferGroupedAssetToUtxoParser parser = new TransferGroupedAssetToUtxoParser(_identityKeyProvidersRegistry);
			TransferGroupedAssetToUtxo block = (TransferGroupedAssetToUtxo)parser.Parse(packet);

			Assert.Equal(syncBlockHeight, block.SyncBlockHeight);
			Assert.Equal(nonce, block.Nonce);
			Assert.Equal(powHash, block.PowHash);
			Assert.Equal(version, block.Version);
			Assert.Equal(blockHeight, block.BlockHeight);
			Assert.Equal(uptodateFunds, block.UptodateFunds);
			Assert.Equal(destinationKey, block.DestinationKey);
			Assert.Equal(transactionPublicKey, block.TransactionPublicKey);
			Assert.Equal(assetCommitment, block.TransferredAsset.AssetCommitment);
			Assert.Equal(blindedAssetsGroupsCount, block.BlindedAssetsGroups.Length);
			Assert.Equal(blindedAssetsGroupsCount * assetsCount + 1, block.InversedSurjectionProofs.Length);

			//for (int i = 0; i < assetsCount; i++)
			//{
			//    Assert.Equal(assetCommitments[i], block.SurjectionProof.AssetCommitments[i]);
			//    Assert.Equal(s[i], block.SurjectionProof.Rs.S[i]);
			//}
			//Assert.Equal(e, block.SurjectionProof.Rs.E);
			Assert.Equal(mask, block.TransferredAsset.EcdhTuple.Mask);
			Assert.Equal(assetId, block.TransferredAsset.EcdhTuple.AssetId);
			Assert.Equal(_publicKey, block.Signer.Value.ToArray());
			Assert.Equal(expectedSignature, block.Signature.ToArray());
		}

		[Fact]
		public void IssueAssetTest()
		{
			ulong tagId = 147;
			ulong syncBlockHeight = 1;
			uint nonce = 4;
			byte[] powHash = BinaryHelper.GetPowHash(1234);
			ushort version = 1;
			ulong blockHeight = 9;
			byte[] body;
			ulong uptodateFunds = 10001;

			byte[] assetId = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] assetCommitment = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] mask = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] maskedAssetId = ConfidentialAssetsHelper.GetRandomSeed();

			using (MemoryStream ms = new MemoryStream())
			{
				using (BinaryWriter bw = new BinaryWriter(ms))
				{
					bw.Write(tagId);
					bw.Write(uptodateFunds);
					bw.Write(assetId);
					bw.Write(assetCommitment);
					bw.Write(mask);
					bw.Write(maskedAssetId);
				}

				body = ms.ToArray();
			}
		}

		[Fact]
		public void IssueBlindedAssetParserTest()
		{
			byte[] groupId = ConfidentialAssetsHelper.GetRandomSeed();
			ulong syncBlockHeight = 1;
			uint nonce = 4;
			byte[] powHash = BinaryHelper.GetPowHash(1234);
			ushort version = 1;
			ulong blockHeight = 9;
			byte[] body;
			ulong uptodateFunds = 10001;

			byte[] assetCommitment = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] mask = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] maskedAssetId = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] keyImage = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] c = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] r = ConfidentialAssetsHelper.GetRandomSeed();

			using (MemoryStream ms = new MemoryStream())
			{
				using (BinaryWriter bw = new BinaryWriter(ms))
				{
					bw.Write(uptodateFunds);
					bw.Write(groupId);
					bw.Write(assetCommitment);
					//bw.Write(mask);
					//bw.Write(maskedAssetId);
					bw.Write(keyImage);
					bw.Write(c);
					bw.Write(r);
				}

				body = ms.ToArray();
			}

			byte[] packet = BinaryHelper.GetSignedPacket(PacketType.Transactional, syncBlockHeight, nonce, powHash, version,
				BlockTypes.Transaction_TransferGroupedAssetsToUtxo, blockHeight, null, body, _privateKey, out byte[] expectedSignature);

			IssueBlindedAssetParser parser = new IssueBlindedAssetParser(_identityKeyProvidersRegistry);
			IssueBlindedAsset block = (IssueBlindedAsset)parser.Parse(packet);

			Assert.Equal(syncBlockHeight, block.SyncBlockHeight);
			Assert.Equal(nonce, block.Nonce);
			Assert.Equal(powHash, block.PowHash);
			Assert.Equal(version, block.Version);
			Assert.Equal(blockHeight, block.BlockHeight);
			Assert.Equal(uptodateFunds, block.UptodateFunds);
			Assert.Equal(groupId, block.GroupId);
			Assert.Equal(assetCommitment, block.AssetCommitment);
			Assert.Equal(keyImage, block.KeyImage);
			Assert.Equal(c, block.UniquencessProof.C);
			Assert.Equal(r, block.UniquencessProof.R);
			Assert.Equal(expectedSignature, block.Signature.ToArray());
		}

		[Fact]
		public void TransferAssetToUtxoParserTest()
		{
			ulong syncBlockHeight = 1;
			uint nonce = 4;
			byte[] powHash = BinaryHelper.GetPowHash(1234);
			ushort version = 1;
			ulong blockHeight = 9;
			ulong uptodateFunds = 10002;

			byte[] destinationKey = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] transactionKey = ConfidentialAssetsHelper.GetRandomSeed();

			byte[] body;

			byte[] assetCommitment = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] mask = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] maskedAssetId = ConfidentialAssetsHelper.GetRandomSeed();

			ushort assetCommitmentsCount = 10;
			byte[][] assetCommitments = new byte[assetCommitmentsCount][];
			byte[] e = ConfidentialAssetsHelper.GetRandomSeed();
			byte[][] s = new byte[assetCommitmentsCount][];

			for (int i = 0; i < assetCommitmentsCount; i++)
			{
				assetCommitments[i] = ConfidentialAssetsHelper.GetRandomSeed();
				s[i] = ConfidentialAssetsHelper.GetRandomSeed();
			}

			using (MemoryStream ms = new MemoryStream())
			{
				using (BinaryWriter bw = new BinaryWriter(ms))
				{
					bw.Write(uptodateFunds);
					bw.Write(destinationKey);
					bw.Write(transactionKey);
					bw.Write(assetCommitment);
					bw.Write(maskedAssetId);
					bw.Write(mask);
					bw.Write(assetCommitmentsCount);
					for (int i = 0; i < assetCommitmentsCount; i++)
					{
						bw.Write(assetCommitments[i]);
					}
					bw.Write(e);
					for (int i = 0; i < assetCommitmentsCount; i++)
					{
						bw.Write(s[i]);
					}
				}

				body = ms.ToArray();
			}

			byte[] packet = BinaryHelper.GetSignedPacket(
				PacketType.Synchronization,
				syncBlockHeight,
				nonce, powHash, version,
				BlockTypes.Transaction_TransferAssetToUtxo, blockHeight, null, body, _privateKey, out byte[] expectedSignature);

			TransferAssetToUtxoParser parser = new TransferAssetToUtxoParser(_identityKeyProvidersRegistry);
			TransferAssetToUtxo block = (TransferAssetToUtxo)parser.Parse(packet);

			Assert.Equal(syncBlockHeight, block.SyncBlockHeight);
			Assert.Equal(nonce, block.Nonce);
			Assert.Equal(powHash, block.PowHash);
			Assert.Equal(version, block.Version);
			Assert.Equal(blockHeight, block.BlockHeight);
			Assert.Equal(destinationKey, block.DestinationKey);
			Assert.Equal(transactionKey, block.TransactionPublicKey);
			Assert.Equal(assetCommitment, block.TransferredAsset.AssetCommitment);
			Assert.Equal(maskedAssetId, block.TransferredAsset.EcdhTuple.AssetId);
			Assert.Equal(mask, block.TransferredAsset.EcdhTuple.Mask);
			Assert.Equal(assetCommitmentsCount, block.SurjectionProof.AssetCommitments.Length);
			Assert.Equal(e, block.SurjectionProof.Rs.E);

			for (int i = 0; i < assetCommitmentsCount; i++)
			{
				Assert.Equal(assetCommitments[i], block.SurjectionProof.AssetCommitments[i]);
				Assert.Equal(s[i], block.SurjectionProof.Rs.S[i]);
			}
			Assert.Equal(expectedSignature, block.Signature.ToArray());
		}
	}
}