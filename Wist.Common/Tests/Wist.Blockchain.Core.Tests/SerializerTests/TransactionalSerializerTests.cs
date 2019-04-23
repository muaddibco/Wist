using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wist.Blockchain.Core.DataModel.Transactional;
using Wist.Blockchain.Core.DataModel.Transactional.Internal;
using Wist.Blockchain.Core.Enums;
using Wist.Blockchain.Core.Serializers.Signed.Transactional;
using Wist.Core.Cryptography;
using Wist.Core.ExtensionMethods;
using Wist.Core.Identity;
using Wist.Crypto.ConfidentialAssets;
using Wist.Tests.Core;
using Xunit;

namespace Wist.Blockchain.Core.Tests.SerializerTests
{
	public class TransactionalSerializerTests : TestBase
	{
		public TransactionalSerializerTests() : base()
		{

		}

		[Fact]
		public void TransferFundsBlockSerializerTests()
		{
			ulong syncBlockHeight = 1;
			uint nonce = 4;
			byte[] powHash = BinaryHelper.GetPowHash(1234);
			ushort version = 1;
			ulong blockHeight = 9;
			byte[] body;

			ulong uptodateFunds = 10001;
			byte[] targetHash = BinaryHelper.GetDefaultHash(1235);

			using (MemoryStream ms = new MemoryStream())
			{
				using (BinaryWriter bw = new BinaryWriter(ms))
				{
					bw.Write(uptodateFunds);
					bw.Write(targetHash);
				}

				body = ms.ToArray();
			}

			byte[] expectedPacket = BinaryHelper.GetSignedPacket(PacketType.Transactional, syncBlockHeight, nonce, powHash, version,
				BlockTypes.Transaction_TransferFunds, blockHeight, null, body, _privateKey, out byte[] expectedSignature);

			TransferFundsBlock block = new TransferFundsBlock
			{
				SyncBlockHeight = syncBlockHeight,
				Nonce = nonce,
				PowHash = powHash,
				BlockHeight = blockHeight,
				UptodateFunds = uptodateFunds,
				TargetOriginalHash = targetHash
			};

			TransferFundsBlockSerializer serializer = new TransferFundsBlockSerializer();
			serializer.Initialize(block);
			serializer.SerializeBody();
			_signingService.Sign(block);

			byte[] actualPacket = serializer.GetBytes();

			Assert.Equal(expectedPacket, actualPacket);
		}

		[Fact]
		public void IssueAssetsBlockSerializerTest()
		{
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

			byte[] expectedPacket = BinaryHelper.GetSignedPacket(PacketType.Transactional, syncBlockHeight, nonce, powHash, version,
				BlockTypes.Transaction_IssueGroupedAssets, blockHeight, null, body, _privateKey, out byte[] expectedSignature);

			AssetsIssuanceGroup[] assetsIssuanceGroups = new AssetsIssuanceGroup[issuanceNonBlindedGroupsCount];
			for (int i = 0; i < issuanceNonBlindedGroupsCount; i++)
			{
				assetsIssuanceGroups[i] = new AssetsIssuanceGroup
				{
					GroupId = issuanceNonBlindedGroupIds[i],
					AssetIssuances = new AssetIssuance[issuedNonBlindedAssetsCount]
				};

				for (int j = 0; j < issuedNonBlindedAssetsCount; j++)
				{
					assetsIssuanceGroups[i].AssetIssuances[j] = new AssetIssuance
					{
						AssetId = issuedNonBlindedAssetIds[i][j],
						IssuedAssetInfo = issuedNonBlindedAssetInfos[i][j]
					};
				}
			}

			BlindedAssetsIssuanceGroup[] blindedAssetsIssuanceGroups = new BlindedAssetsIssuanceGroup[issuanceBlindedGroupsCount];
			for (int i = 0; i < issuanceBlindedGroupsCount; i++)
			{
				blindedAssetsIssuanceGroups[i] = new BlindedAssetsIssuanceGroup
				{
					GroupId = issuanceBlindedGroupIds[i],
					AssetCommitments = issuedAssetCommitments[i],
					AssetIssuances = new AssetIssuance[issuedBlindedAssetsCount],
					IssuanceProofs = new IssuanceProof[issuedBlindedAssetsCount]
				};

				for (int j = 0; j < issuedBlindedAssetsCount; j++)
				{
					blindedAssetsIssuanceGroups[i].AssetIssuances[j] = new AssetIssuance
					{
						AssetId = issuedBlindedAssetIds[i][j],
						IssuedAssetInfo = issuedBlindedAssetInfos[i][j]
					};

					blindedAssetsIssuanceGroups[i].IssuanceProofs[j] = new IssuanceProof
					{
						SurjectionProof = new SurjectionProof
						{
							AssetCommitments = new byte[][] { surjectionProofCommitments[i][j] },
							Rs = new BorromeanRingSignature
							{
								E = surjectionProofE[i][j],
								S = new byte[][] { surjectionProofS[i][j] }
							}
						},
						Mask = issuanceProofMask[i][j]
					};
				}
			}

			AssetsGroup[] assetsGroups = new AssetsGroup[ownedNonBlindedAssetGroupsCount];
			for (int i = 0; i < ownedNonBlindedAssetGroupsCount; i++)
			{
				assetsGroups[i] = new AssetsGroup
				{
					GroupId = nonBlindedGroupIds[i],
					AssetIds = ownedAssetIds[i],
					AssetAmounts = ownedAssetAmounts[i]
				};
			}

			BlindedAssetsGroup[] blindedAssetsGroups = new BlindedAssetsGroup[ownedBlindedAssetGroupsCount];
			for (int i = 0; i < ownedBlindedAssetGroupsCount; i++)
			{
				blindedAssetsGroups[i] = new BlindedAssetsGroup
				{
					GroupId = blindedGroupIds[i],
					AssetCommitments = ownedAssetCommitments[i]
				};
			}

			IssueGroupedAssets block = new IssueGroupedAssets
			{
				SyncBlockHeight = syncBlockHeight,
				Nonce = nonce,
				PowHash = powHash,
				BlockHeight = blockHeight,
				UptodateFunds = uptodateFunds,
				AssetsIssuanceGroups = assetsIssuanceGroups,
				BlindedAssetsIssuanceGroups = blindedAssetsIssuanceGroups,
				AssetsGroups = assetsGroups,
				BlindedAssetsGroups = blindedAssetsGroups,
				IssuanceInfo = issuanceInfo
			};

			IssueAssetsSerializer serializer = new IssueAssetsSerializer();
			serializer.Initialize(block);
			serializer.SerializeBody();
			_signingService.Sign(block);

			byte[] actualPacket = serializer.GetBytes();

			Assert.Equal(expectedPacket, actualPacket);
		}

		[Fact]
		public void TransferGroupedAssetsToUtxoBlockSerializerTest()
		{
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

			byte[] expectedPacket = BinaryHelper.GetSignedPacket(PacketType.Transactional, syncBlockHeight, nonce, powHash, version,
				BlockTypes.Transaction_TransferGroupedAssetsToUtxo, blockHeight, null, body, _privateKey, out byte[] expectedSignature);

			EncryptedAsset transferredAsset = new EncryptedAsset
			{
				AssetCommitment = assetCommitment,
				EcdhTuple = new EcdhTupleCA
				{
					AssetId = assetId,
					Mask = mask
				}
			};

			BlindedAssetsGroup[] blindedAssetsGroups = new BlindedAssetsGroup[blindedAssetsGroupsCount];
			for (int i = 0; i < blindedAssetsGroupsCount; i++)
			{
				blindedAssetsGroups[i] = new BlindedAssetsGroup
				{
					GroupId = groupIds[i],
					AssetCommitments = ownedAssetCommitments[i]
				};
			}

			InversedSurjectionProof[] inversedSurjectionProofs = new InversedSurjectionProof[blindedAssetsGroupsCount * assetsCount + 1];
			for (int i = 0; i < blindedAssetsGroupsCount * assetsCount + 1; i++)
			{
				inversedSurjectionProofs[i] = new InversedSurjectionProof
				{
					AssetCommitment = surjectionAssetCommitment[i],
					Rs = new BorromeanRingSignature
					{
						E = e[i],
						S = s[i]
					}
				};
			}

			TransferGroupedAssetToUtxo block = new TransferGroupedAssetToUtxo
			{
				SyncBlockHeight = syncBlockHeight,
				Nonce = nonce,
				PowHash = powHash,
				BlockHeight = blockHeight,
				UptodateFunds = uptodateFunds,
				DestinationKey = destinationKey,
				TransactionPublicKey = transactionPublicKey,
				TransferredAsset = transferredAsset,
				BlindedAssetsGroups = blindedAssetsGroups,
				InversedSurjectionProofs = inversedSurjectionProofs
			};

			TransferGroupedAssetsToUtxoSerializer serializer = new TransferGroupedAssetsToUtxoSerializer();
			serializer.Initialize(block);
			serializer.SerializeBody();
			_signingService.Sign(block);

			byte[] actualPacket = serializer.GetBytes();

			Assert.Equal(expectedPacket, actualPacket);
		}

		[Fact]
		public void IssueBlindedAssetSerializerTest()
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
					bw.Write(keyImage);
					bw.Write(c);
					bw.Write(r);
				}

				body = ms.ToArray();
			}

			byte[] expectedPacket = BinaryHelper.GetSignedPacket(PacketType.Transactional, syncBlockHeight, nonce, powHash, version,
				BlockTypes.Transaction_IssueBlindedAsset, blockHeight, null, body, _privateKey, out byte[] expectedSignature);

			IssueBlindedAsset issueBlindedAsset = new IssueBlindedAsset
			{
				SyncBlockHeight = syncBlockHeight,
				Nonce = nonce,
				PowHash = powHash,
				BlockHeight = blockHeight,
				GroupId = groupId,
				UptodateFunds = uptodateFunds,
				AssetCommitment = assetCommitment,
				KeyImage = keyImage,
				UniquencessProof = new RingSignature
				{
					 C = c,
					 R = r
				}
			};

			IssueBlindedAssetSerializer serializer = new IssueBlindedAssetSerializer();
			serializer.Initialize(issueBlindedAsset);
			serializer.SerializeBody();
			_signingService.Sign(issueBlindedAsset);

			byte[] actualPacket = serializer.GetBytes();

			Assert.Equal(expectedPacket, actualPacket);
		}

		[Fact]
		public void TransferAssetToUtxoSerializerTest()
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

			byte[] expectedPacket = BinaryHelper.GetSignedPacket(PacketType.Transactional, syncBlockHeight, nonce, powHash, version,
				BlockTypes.Transaction_TransferAssetToUtxo, blockHeight, null, body, _privateKey, out byte[] expectedSignature);

			TransferAssetToUtxo packet = new TransferAssetToUtxo
			{
				SyncBlockHeight = syncBlockHeight,
				Nonce = nonce,
				PowHash = powHash,
				BlockHeight = blockHeight,
				UptodateFunds = uptodateFunds,
				DestinationKey = destinationKey, 
				TransactionPublicKey = transactionKey,
				TransferredAsset = new EncryptedAsset
				{
					AssetCommitment = assetCommitment,
					EcdhTuple = new EcdhTupleCA
					{
						Mask = mask,
						AssetId = maskedAssetId
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

			TransferAssetToUtxoSerializer serializer = new TransferAssetToUtxoSerializer();
			serializer.Initialize(packet);
			serializer.SerializeBody();
			_signingService.Sign(packet);

			byte[] actualPacket = serializer.GetBytes();

			Assert.Equal(expectedPacket, actualPacket);
		}
	}
}
