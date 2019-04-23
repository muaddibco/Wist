using Chaos.NaCl;
using HashLib;
using System;
using System.IO;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Cryptography;
using Wist.Core.ExtensionMethods;
using Wist.Crypto.ConfidentialAssets;

namespace Wist.Tests.Core
{
	public class BinaryHelper
    {
        public static byte STX = 2;
        public static byte DLE = 10;

        private readonly byte[] _defaultPrivateKey;

        public BinaryHelper(byte[] defaultPrivateKey)
        {
            _defaultPrivateKey = defaultPrivateKey;
        }

        public byte[] GetSignedPacket(PacketType packetType, ulong syncBlockHeight, uint nonce, byte[] powHash, ushort version, ushort blockType, ulong blockHeight, byte[] prevHash, byte[] body, out byte[] signature)
        {
            return GetSignedPacket(packetType, syncBlockHeight, nonce, powHash, version, blockType, blockHeight, prevHash, body, _defaultPrivateKey, out signature);
        }

        public static byte[] GetSignedPacket(PacketType packetType, ulong syncBlockHeight, uint nonce, byte[] powHash, ushort version, ushort blockType, ulong blockHeight, byte[] prevHash, byte[] body, byte[] privateKey, out byte[] signature)
        {
            byte[] result = null;

            byte[] bodyBytes = null;

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write((ushort)packetType);
                    bw.Write(syncBlockHeight);
                    bw.Write(nonce);
                    bw.Write(powHash);
                    bw.Write(version);
                    bw.Write(blockType);
                    bw.Write(blockHeight);
                    if (prevHash != null)
                    {
                        bw.Write(prevHash);
                    }
                    bw.Write(body);
                }

                bodyBytes = ms.ToArray();
            }

            byte[] publicKey = Ed25519.PublicKeyFromSeed(privateKey);
            byte[] expandedPrivateKey = Ed25519.ExpandedPrivateKeyFromSeed(privateKey);
            signature = Ed25519.Sign(bodyBytes, expandedPrivateKey);

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(bodyBytes);
                    bw.Write(signature);
                    bw.Write(publicKey);
                }

                result = ms.ToArray();
            }

            return result;
        }

		public static byte[] GetUtxoConfidentialPacket(PacketType packetType, ulong syncBlockHeight, uint nonce, byte[] powHash, ushort version, ushort blockType, byte[] keyImage, byte[] destinationKey, byte[] destinationKey2, byte[] transactionPublicKey, byte[] body, byte[][] pubKeys, byte[] secretKey, int secIndex, out RingSignature[] ringSignatures)
        {
            byte[] bodyBytes = null;
            byte[] result = null;

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write((ushort)packetType);
                    bw.Write(syncBlockHeight);
                    bw.Write(nonce);
                    bw.Write(powHash);
                    bw.Write(version);
                    bw.Write(blockType);
                    bw.Write(destinationKey);
                    bw.Write(destinationKey2);
					bw.Write(transactionPublicKey);
                    bw.Write(body);
                    bw.Write(keyImage);
                }

                bodyBytes = ms.ToArray();
            }

            ringSignatures = ConfidentialAssetsHelper.GenerateRingSignature(bodyBytes, keyImage, pubKeys, secretKey, secIndex);
            
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(bodyBytes);
                    bw.Write((ushort)ringSignatures.Length);
                    for (int i = 0; i < pubKeys.Length; i++)
                    {
                        bw.Write(pubKeys[i]);
                    }
                    for (int i = 0; i < pubKeys.Length; i++)
                    {
                        bw.Write(ringSignatures[i].C);
                        bw.Write(ringSignatures[i].R);
                    }
                }

                result = ms.ToArray();
            }

            return result;
        }

        public static byte[] GetRandomPublicKey()
        {
            byte[] seed = ConfidentialAssetsHelper.GetRandomSeed();

            return Ed25519.PublicKeyFromSeed(seed);
        }

        public static byte[] GetRandomPublicKey(out byte[] secretKey)
        {
            secretKey = ConfidentialAssetsHelper.GetRandomSeed();

            return Ed25519.PublicKeyFromSeed(secretKey);
        }

        public static byte[] GetTransactionKeyHash(int forValue = 0)
        {
            IHash hash = HashLib.HashFactory.Hash128.CreateMurmur3_128();
            byte[] originBytes = BitConverter.GetBytes(forValue);
            byte[] hashBytes = hash.ComputeBytes(originBytes).GetBytes();

            return hashBytes;
        }

        public static byte[] GetPowHash(int forValue = 0)
        {
            IHash hash = HashLib.HashFactory.Crypto.CreateTiger_4_192();
            byte[] powHashOrigin = BitConverter.GetBytes(forValue);
            byte[] powHash = hash.ComputeBytes(powHashOrigin).GetBytes();

            return powHash;
        }

        public static byte[] GetDefaultHash(int forValue = 0)
        {
            IHash hash = HashLib.HashFactory.Crypto.CreateSHA256();
            byte[] hashOrigin = BitConverter.GetBytes(forValue);
            byte[] hashBytes = hash.ComputeBytes(hashOrigin).GetBytes();

            return hashBytes;
        }

        public static byte[] GetDefaultHash(byte[] bytes)
        {
            IHash hash = HashLib.HashFactory.Crypto.CreateSHA256();
            byte[] hashBytes = hash.ComputeBytes(bytes).GetBytes();

            return hashBytes;
        }
    }
}
