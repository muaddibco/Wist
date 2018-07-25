using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Logging;
using Wist.Core.Models;
using Wist.Core.ProofOfWork;
using Wist.Core.States;
using Wist.Core.Synchronization;

namespace Wist.BlockLattice.Core.Handlers
{
    [RegisterExtension(typeof(IPacketVerifier), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class SignaturedPacketTypeHandler : PowBasedPacketVerifierBase
    {
        public const int MESSAGE_TYPE_SIZE = 2;
        public const int MESSAGE_SIGNATURE_SIZE = 64;
        public const int MESSAGE_PUBLICKEY_SIZE = 32;

        public SignaturedPacketTypeHandler(IStatesRepository statesRepository, IProofOfWorkCalculationRepository proofOfWorkCalculationRepository, ILoggerService loggerService) 
            : base(statesRepository, loggerService, proofOfWorkCalculationRepository)
        {
        }

        public override PacketType PacketType => PacketType.AccountChain;

        protected override bool ValidatePacket(BinaryReader br, uint syncBlockHeight)
        {
            ushort messageType = br.ReadUInt16();

            int actualMessageBodyLength = (int)(br.BaseStream.Length - (MESSAGE_TYPE_SIZE + MESSAGE_SIGNATURE_SIZE + MESSAGE_PUBLICKEY_SIZE));

            byte[] messageBody = br.ReadBytes(actualMessageBodyLength);
            byte[] signature = br.ReadBytes(MESSAGE_SIGNATURE_SIZE);
            byte[] publickKey = br.ReadBytes(MESSAGE_PUBLICKEY_SIZE);

            if (!VerifySignature(messageBody, signature, publickKey))
            {
                _log.Error("Signature is invalid");
                return false;
            }

            return true;
        }

        private bool VerifySignature(byte[] messageBody, byte[] signature, byte[] publicKey)
        {
            //TODO: Add signature verification
            return true;
        }
    }
}
