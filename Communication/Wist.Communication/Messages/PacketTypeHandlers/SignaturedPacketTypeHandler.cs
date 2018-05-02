using Wist.Communication.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Models;

namespace Wist.Communication.Messages.PacketTypeHandlers
{
    [RegisterExtension(typeof(IPacketTypeHandler), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class SignaturedPacketTypeHandler : IPacketTypeHandler
    {
        public PacketTypes PacketType => PacketTypes.Signatured;
        public const int MESSAGE_TYPE_SIZE = 2;
        public const int MESSAGE_LENGTH_SIZE = 2;
        public const int MESSAGE_SIGNATURE_SIZE = 64;
        public const int MESSAGE_PUBLICKEY_SIZE = 32;

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

                    int actualMessageBodyLength = (int)(messagePacket.Length - (MESSAGE_TYPE_SIZE + MESSAGE_LENGTH_SIZE + MESSAGE_SIGNATURE_SIZE + MESSAGE_PUBLICKEY_SIZE));
                    if (actualMessageBodyLength != length)
                    {
                        return new PacketErrorMessage(PacketsErrors.LENGTH_DOES_NOT_MATCH, messagePacket);
                    }

                    byte[] messageBody = br.ReadBytes(length);
                    byte[] signature = br.ReadBytes(MESSAGE_SIGNATURE_SIZE);
                    byte[] publickKey = br.ReadBytes(MESSAGE_PUBLICKEY_SIZE);

                    if (!VerifySignature(messageBody, signature, publickKey))
                    {
                        return new PacketErrorMessage(PacketsErrors.SIGNATURE_IS_INVALID, messagePacket);
                    }


                }
            }

            return await Task.FromResult(new PacketErrorMessage(PacketsErrors.NO_ERROR));
        }

        private bool VerifySignature(byte[] messageBody, byte[] signature, byte[] publicKey)
        {
            return true;
        }
    }
}
