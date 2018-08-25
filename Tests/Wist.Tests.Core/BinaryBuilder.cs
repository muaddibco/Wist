using Chaos.NaCl;
using HashLib;
using System;
using System.IO;
using System.Security.Cryptography;
using Wist.BlockLattice.Core;
using Wist.BlockLattice.Core.Enums;

namespace Wist.Tests.Core
{
    public class BinaryBuilder
    {
        public static byte STX = 2;
        public static byte DLE = 10;

        private readonly byte[] _defaultPrivateKey;

        public BinaryBuilder(byte[] defaultPrivateKey)
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
                    bw.Write(version);
                    bw.Write(blockType);
                    bw.Write(blockHeight);
                    bw.Write(prevHash);
                    bw.Write(body);
                }

                bodyBytes = ms.ToArray();
            }

            byte[] publickKey = Ed25519.PublicKeyFromSeed(privateKey);
            byte[] expandedPrivateKey = Ed25519.ExpandedPrivateKeyFromSeed(privateKey);
            signature = Ed25519.Sign(bodyBytes, expandedPrivateKey);

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    //bw.Write(DLE);
                    //bw.Write(STX);

                    //uint length = (uint)(sizeof(PacketType) + sizeof(ulong) + sizeof(uint) + powHash.Length + sizeof(ushort) + sizeof(ushort) + sizeof(ulong) + prevHash.Length + body.Length + publickKey.Length + signature.Length);
                    //bw.Write(length);

                    bw.Write((ushort)packetType);
                    bw.Write(syncBlockHeight);
                    bw.Write(nonce);
                    bw.Write(powHash);
                    bw.Write(bodyBytes);
                    bw.Write(signature);
                    bw.Write(publickKey);
                }

                result = ms.ToArray();
            }

            return result;
        }

        public static byte[] GetRandomSeed()
        {
            byte[] seed = new byte[32];
            RNGCryptoServiceProvider.Create().GetNonZeroBytes(seed);

            return seed;
        }

        public static byte[] GetPowHash(int forValue = 0)
        {
            IHash hash = HashLib.HashFactory.Crypto.CreateTiger_4_192();
            byte[] powHashOrigin = BitConverter.GetBytes(forValue);
            byte[] powHash = hash.ComputeBytes(powHashOrigin).GetBytes();

            return powHash;
        }

        public static byte[] GetPrevHash(int forValue = 0)
        {
            IHash hash = HashLib.HashFactory.Crypto.CreateSHA256();
            byte[] hashOrigin = BitConverter.GetBytes(forValue);
            byte[] hashBytes = hash.ComputeBytes(hashOrigin).GetBytes();

            return hashBytes;
        }
    }
}
