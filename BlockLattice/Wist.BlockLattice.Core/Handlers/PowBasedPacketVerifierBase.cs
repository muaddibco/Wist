using System;
using System.Buffers.Binary;
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
        private readonly IProofOfWorkCalculationRepository _proofOfWorkCalculationRepository;

        public PowBasedPacketVerifierBase(IStatesRepository statesRepository, ILoggerService loggerService, IProofOfWorkCalculationRepository proofOfWorkCalculationFactory) 
            : base(statesRepository, loggerService)
        {
            _proofOfWorkCalculationRepository = proofOfWorkCalculationFactory;
        }

        protected override bool ValidatePacket(Span<byte> span, ulong syncBlockHeight)
        {
            Span<byte> spanAfterPow;
            bool powIsCorrect = CheckSyncPOW(span, syncBlockHeight, out spanAfterPow);

            if (powIsCorrect)
            {
                bool packetIsCorrect = ValidatePacketAfterPow(spanAfterPow, syncBlockHeight);

                return packetIsCorrect;
            }

            return false;
        }

        private bool CheckSyncPOW(Span<byte> span, ulong syncBlockHeight, out Span<byte> spanOut)
        {
            ushort powTypeValue =  BinaryPrimitives.ReadUInt16LittleEndian(span);
            POWType pOWType = (POWType)powTypeValue;

            if (pOWType != POWType.None)
            {
                //TODO: Add difficulty check

                IProofOfWorkCalculation proofOfWorkCalculation = _proofOfWorkCalculationRepository.Create(pOWType);

                try
                {
                    ulong nonce = BinaryPrimitives.ReadUInt64LittleEndian(span.Slice(2));
                    byte[] hash = span.Slice(10, proofOfWorkCalculation.HashSize).ToArray();

                    BigInteger bigInteger = new BigInteger((syncBlockHeight == _synchronizationContext.LastBlockDescriptor.BlockHeight) ? _synchronizationContext.LastBlockDescriptor.Hash : _synchronizationContext.PrevBlockDescriptor.Hash);
                    bigInteger += nonce;

                    byte[] input = bigInteger.ToByteArray();
                    byte[] computedHash = proofOfWorkCalculation.CalculateHash(input);

                    spanOut = span.Slice(10 + proofOfWorkCalculation.HashSize);

                    if (!computedHash.EqualsX16(hash))
                    {
                        _log.Error("Computed HASH differs from obtained one");
                        return false;
                    }

                }
                finally
                {
                    if (proofOfWorkCalculation != null)
                    {
                        _proofOfWorkCalculationRepository.Utilize(proofOfWorkCalculation);
                    }
                }
            }
            else
            {
                spanOut = span.Slice(2);
            }

            return true;
        }

        protected abstract bool ValidatePacketAfterPow(Span<byte> span, ulong syncBlockHeight);
    }
}
