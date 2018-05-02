using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Models;

namespace Wist.BlockLattice.Core.Handlers
{
    [RegisterExtension(typeof(IChainTypeValidationHandler), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class SignaturedPacketTypeHandler : PacketTypeHandlerBase
    {
        public const int MESSAGE_TYPE_SIZE = 2;
        public const int MESSAGE_LENGTH_SIZE = 2;
        public const int MESSAGE_SIGNATURE_SIZE = 64;
        public const int MESSAGE_PUBLICKEY_SIZE = 32;

        public override ChainType ChainType => ChainType.AccountChain;

        protected override PacketsErrors ProcessPacket(BinaryReader br)
        {
            ushort messageType = br.ReadUInt16();
            ushort length = br.ReadUInt16();

            if (length == 0)
            {
                return PacketsErrors.LENGTH_IS_INVALID;
            }

            int actualMessageBodyLength = (int)(br.BaseStream.Length - (MESSAGE_TYPE_SIZE + MESSAGE_LENGTH_SIZE + MESSAGE_SIGNATURE_SIZE + MESSAGE_PUBLICKEY_SIZE));
            if (actualMessageBodyLength != length)
            {
                return PacketsErrors.LENGTH_DOES_NOT_MATCH;
            }

            byte[] messageBody = br.ReadBytes(length);
            byte[] signature = br.ReadBytes(MESSAGE_SIGNATURE_SIZE);
            byte[] publickKey = br.ReadBytes(MESSAGE_PUBLICKEY_SIZE);

            if (!VerifySignature(messageBody, signature, publickKey))
            {
                return PacketsErrors.SIGNATURE_IS_INVALID;
            }


            return PacketsErrors.NO_ERROR;
        }

        private bool VerifySignature(byte[] messageBody, byte[] signature, byte[] publicKey)
        {
            return true;
        }
    }
}
