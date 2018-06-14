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
using Wist.Core.Logging;
using Wist.Core.ProofOfWork;
using Wist.Core.Synchronization;
using Wist.Node.Core.Interfaces;

namespace Wist.Node.Core.Parsers
{
    [RegisterExtension(typeof(IPacketVerifier), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class ConsensusPacketTypeHandler : PacketVerifierBase
    {
        public const int MESSAGE_TYPE_SIZE = 2;
        public const int MESSAGE_LENGTH_SIZE = 2;
        public const int MESSAGE_SIGNATURE_SIZE = 64;
        public const int MESSAGE_PUBLICKEY_SIZE = 32;

        private readonly IConsensusHub _consensusHub;

        public ConsensusPacketTypeHandler(IConsensusHub consensusHub, ISynchronizationContext synchronizationContext, ILoggerService loggerService)
            : base(synchronizationContext, loggerService)
        {
            _consensusHub = consensusHub;
        }

        public override PacketType PacketType => PacketType.Consensus;

        protected override bool ValidatePacket(BinaryReader br, uint syncBlockHeight)
        {
            ushort messageType = br.ReadUInt16();
            ushort length = br.ReadUInt16();

            if (length == 0)
            {
                _log.Error("Length is invalid");
                return false;
            }

            int actualMessageBodyLength = (int)(br.BaseStream.Length - (MESSAGE_TYPE_SIZE + MESSAGE_LENGTH_SIZE + MESSAGE_SIGNATURE_SIZE + MESSAGE_PUBLICKEY_SIZE));
            if (actualMessageBodyLength != length)
            {
                _log.Error("Reported length of packet differs from actual one.");
                return false;
            }

            byte[] messageBody = br.ReadBytes(length);
            byte[] signature = br.ReadBytes(MESSAGE_SIGNATURE_SIZE);
            byte[] publickKey = br.ReadBytes(MESSAGE_PUBLICKEY_SIZE);

            if(!VerifyConsensusParticipant(publickKey))
            {
                _log.Error("Invalid consensus group participant");
                return false;
            }

            if (!VerifySignature(messageBody, signature, publickKey))
            {
                _log.Error("Signature is invalid");
                return false;
            }


            return true;
        }

        private bool VerifyConsensusParticipant(byte[] publicKey)
        {
            return _consensusHub.GroupParticipants.ContainsKey(publicKey.ToHexString());
        }

        private bool VerifySignature(byte[] messageBody, byte[] signature, byte[] publicKey)
        {
            //TODO: Add signature verification
            return true;
        }
    }
}
