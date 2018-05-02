using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;

namespace Wist.BlockLattice.Core.Handlers
{
    [RegisterExtension(typeof(IPacketTypeHandler), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class HashBasedPacketTypeHandler : PacketTypeHandlerBase
    {
        public const int MESSAGE_TYPE_SIZE = 2;
        public const int MESSAGE_LENGTH_SIZE = 2;
        public const int MESSAGE_HASH_SIZE = 64;
        public const int MAX_HASH_NBACK = 1000000;

        public override ChainType ChainType => ChainType.TransactionalChain;

        protected override PacketsErrors ProcessPacket(BinaryReader br)
        {
            ushort messageType = br.ReadUInt16();
            ushort length = br.ReadUInt16();

            if (length == 0)
            {
                return PacketsErrors.LENGTH_IS_INVALID;
            }
            
            int actualMessageBodyLength = (int)(br.BaseStream.Length - (2 + MESSAGE_TYPE_SIZE + MESSAGE_LENGTH_SIZE + MESSAGE_HASH_SIZE + MESSAGE_HASH_SIZE));
            if (actualMessageBodyLength != length)
            {
                return PacketsErrors.LENGTH_DOES_NOT_MATCH;
            }

            byte[] messageBody = br.ReadBytes(length);
            byte[] hashOriginal = br.ReadBytes(MESSAGE_HASH_SIZE);
            byte[] hashNBack = br.ReadBytes(MESSAGE_HASH_SIZE);

            if (!VerifyHashNBack(hashOriginal, hashNBack))
            {
                return PacketsErrors.HASHBACK_IS_INVALID;
            }

            return PacketsErrors.NO_ERROR;
        }

        //TODO: weigh real neccessity of such a check. Reason - hashNBack will be chacked later by Consensus Service against last block in chain and if will no match will mean that, probably hashNBack is not valid at all. Such a way will allow to reduce weight of checking hashNBack against original hash value
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
