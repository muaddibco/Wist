using CommunicationLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Models;

namespace CommunicationLibrary.Messages.PacketTypeHandlers
{
    [RegisterExtension(typeof(IPacketTypeHandler), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class SignaturedPacketTypeHandler : IPacketTypeHandler
    {
        public PacketTypes PacketType => PacketTypes.Signatured;

        public async Task<PacketErrorMessage> ProcessPacket(byte[] messagePacket)
        {
            MessageBase msg;
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

                    int actualMessageBodyLength = messagePacket.Length - 4 - 32 - 64;
                    if (actualMessageBodyLength != length)
                    {
                        return new PacketErrorMessage(PacketsErrors.LENGTH_DOES_NOT_MATCH, messagePacket);
                    }

                    byte[] messageBody = br.ReadBytes(length);
                    byte[] signature = br.ReadBytes(64);
                    byte[] publickKey = br.ReadBytes(32);

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
