using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using Wist.Core.ExtensionMethods;
using Wist.Core.Logging;
using Wist.Core.ProofOfWork;
using Wist.Core.States;
using Wist.Core.Synchronization;

namespace Wist.BlockLattice.Core.Handlers
{
    public abstract class PowBasedPacketVerifierBase : PacketVerifierBase
    {
        private readonly IProofOfWorkCalculationFactory _proofOfWorkCalculationFactory;

        public PowBasedPacketVerifierBase(IStatesRepository statesRepository, ILoggerService loggerService, IProofOfWorkCalculationFactory proofOfWorkCalculationFactory) : base(statesRepository, loggerService)
        {
            _proofOfWorkCalculationFactory = proofOfWorkCalculationFactory;
        }

        protected override bool ValidatePacket(BinaryReader br, uint syncBlockHeight)
        {
            return CheckSyncPOW(br, syncBlockHeight);
        }

        private bool CheckSyncPOW(BinaryReader br, uint syncBlockHeight)
        {
            ushort powTypeValue = br.ReadUInt16();
            POWType pOWType = (POWType)powTypeValue;

            IProofOfWorkCalculation proofOfWorkCalculation = _proofOfWorkCalculationFactory.Create(pOWType);

            ulong nonce = br.ReadUInt64();
            byte[] hash = br.ReadBytes(proofOfWorkCalculation.HashSize);

            BigInteger bigInteger = new BigInteger(syncBlockHeight == _synchronizationContext.LastBlockDescriptor.BlockHeight ? _synchronizationContext.LastBlockDescriptor.Hash : _synchronizationContext.PrevBlockDescriptor.Hash);
            bigInteger += nonce;

            byte[] input = bigInteger.ToByteArray();
            byte[] computedHash = proofOfWorkCalculation.CalculateHash(input);

            if (!computedHash.EqualsX16(hash))
            {
                _log.Error("Computed HASH differs from obtained one");
                return false;
            }

            //TODO: Add difficulty check

            return true;
        }
    }
}
