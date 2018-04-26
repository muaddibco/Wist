using CommunicationLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;

namespace CommunicationLibrary.Messages.PacketTypeHandlers
{
    [RegisterExtension(typeof(IPacketTypeHandler), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class HashBasedPacketTypeHandler : IPacketTypeHandler
    {
        public PacketTypes PacketType => PacketTypes.HashBased;

        public const int MESSAGE_TYPE_SIZE = 2;
        public const int MESSAGE_LENGTH_SIZE = 2;
        public const int MESSAGE_HASH_SIZE = 64;
        public const int MAX_HASH_NBACK = 1000000;

        public async Task<PacketErrorMessage> ProcessPacket(byte[] messagePacket)
        {
            using (MemoryStream ms = new MemoryStream(messagePacket))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    ushort messageType = br.ReadUInt16();
                    ushort length = br.ReadUInt16();

                    if (length == 0)
                    {
                        return new PacketErrorMessage(PacketsErrors.LENGTH_IS_INVALID, messagePacket);
                    }

                    int actualMessageBodyLength = (int)(messagePacket.Length - (MESSAGE_TYPE_SIZE + MESSAGE_LENGTH_SIZE + MESSAGE_HASH_SIZE + MESSAGE_HASH_SIZE));
                    if (actualMessageBodyLength != length)
                    {
                        return new PacketErrorMessage(PacketsErrors.LENGTH_DOES_NOT_MATCH, messagePacket);
                    }

                    byte[] messageBody = br.ReadBytes(length);
                    byte[] hashOriginal = br.ReadBytes(MESSAGE_HASH_SIZE);
                    byte[] hashNBack = br.ReadBytes(MESSAGE_HASH_SIZE);

                    if(!VerifyHashNBack(hashOriginal, hashNBack))
                    {
                        return new PacketErrorMessage(PacketsErrors.HASHBACK_IS_INVALID, messagePacket);
                    }
                }
            }

            return await Task.FromResult(new PacketErrorMessage(PacketsErrors.NO_ERROR));
        }

        private bool VerifyHashNBack(byte[] hashOriginal, byte[] hashNBack)
        {
            // if the same hashes were provided so return false
            if (CryptoHelper.HashX16Equals(hashNBack, hashOriginal))
                return false;

            byte[] hash = hashNBack;
            uint loop = 0;
            do
            {
                hash = CryptoHelper.ComputeHash(hash);
            } while (!CryptoHelper.Hash64Equals(hash, hashOriginal) && ++loop < MAX_HASH_NBACK);

            return loop < MAX_HASH_NBACK;
        }
    }
}
