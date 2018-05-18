using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Handlers;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.ExtensionMethods;
using Wist.Core.ProofOfWork;
using Wist.Core.Synchronization;
using Wist.Node.Core.Interfaces;

namespace Wist.Node.Core.Parsers
{
    [RegisterExtension(typeof(IPacketTypeHandler), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class ConsensusPacketTypeHandler : PacketTypeHandlerBase
    {
        public const int MESSAGE_TYPE_SIZE = 2;
        public const int MESSAGE_LENGTH_SIZE = 2;
        public const int MESSAGE_SIGNATURE_SIZE = 64;
        public const int MESSAGE_PUBLICKEY_SIZE = 32;

        private readonly IConsensusHub _consensusHub;

        public ConsensusPacketTypeHandler(IConsensusHub consensusHub, ISynchronizationContext synchronizationContext, IProofOfWorkCalculationFactory proofOfWorkCalculationFactory, IBlockParsersFactory[] blockParsersFactories)
            : base(synchronizationContext, proofOfWorkCalculationFactory, blockParsersFactories)
        {
            _consensusHub = consensusHub;
        }

        public override ChainType ChainType => ChainType.Consensus;

        protected override PacketsErrors ValidatePacket(BinaryReader br)
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

            if(!VerifyConsensusParticipant(publickKey))
            {
                return PacketsErrors.INVALID_CONSENSUS_GROUP_PARTICIPANT;
            }

            if (!VerifySignature(messageBody, signature, publickKey))
            {
                return PacketsErrors.SIGNATURE_IS_INVALID;
            }


            return PacketsErrors.NO_ERROR;
        }

        private bool VerifyConsensusParticipant(byte[] publicKey)
        {
            return _consensusHub.GroupParticipants.ContainsKey(publicKey.ToHexString());
        }

        private bool VerifySignature(byte[] messageBody, byte[] signature, byte[] publicKey)
        {
            // TODO: Add signature verification
            return true;
        }
    }
}
