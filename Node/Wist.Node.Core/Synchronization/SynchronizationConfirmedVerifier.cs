using System;
using System.Buffers.Binary;
using Wist.BlockLattice.Core;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Handlers;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.ExtensionMethods;
using Wist.Core.Logging;
using Wist.Core.ProofOfWork;
using Wist.Core.States;

namespace Wist.Node.Core.Synchronization
{
    [RegisterExtension(typeof(IPacketVerifier), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationConfirmedVerifier : SignaturedBasedPacketVerifierBase
    {
        public SynchronizationConfirmedVerifier(IStatesRepository statesRepository, IProofOfWorkCalculationRepository proofOfWorkCalculationRepository, ICryptoService cryptoService, ILoggerService loggerService) 
            : base(statesRepository, proofOfWorkCalculationRepository, cryptoService, loggerService)
        {
        }

        public override PacketType PacketType => PacketType.Synchronization;

        protected override bool ValidatePackerAfterSignature(Span<byte> span, ulong syncBlockHeight, byte[] publicKey)
        {
            ushort version = BinaryPrimitives.ReadUInt16LittleEndian(span);
            ushort blockType = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(2));
            ulong blockHeight = BinaryPrimitives.ReadUInt64LittleEndian(span.Slice(4));
            byte[] prevHash = span.Slice(12, Globals.HASH_SIZE).ToArray();

            if(blockType == BlockTypes.Synchronization_ConfirmedBlock && version == 1)
            {
                if (_synchronizationContext.LastBlockDescriptor != null && _synchronizationContext.LastBlockDescriptor.BlockHeight + 1 <= blockHeight || _synchronizationContext.LastBlockDescriptor == null)
                {
                    if (_synchronizationContext.LastBlockDescriptor != null && prevHash.Equals64(_synchronizationContext.LastBlockDescriptor.Hash) ||
                        _synchronizationContext.LastBlockDescriptor == null
                        && BinaryPrimitives.ReadUInt64LittleEndian(span.Slice(12)) == 0 && BinaryPrimitives.ReadUInt64LittleEndian(span.Slice(20)) == 0
                        && BinaryPrimitives.ReadUInt64LittleEndian(span.Slice(28)) == 0 && BinaryPrimitives.ReadUInt64LittleEndian(span.Slice(36)) == 0
                        && BinaryPrimitives.ReadUInt64LittleEndian(span.Slice(44)) == 0 && BinaryPrimitives.ReadUInt64LittleEndian(span.Slice(52)) == 0
                        && BinaryPrimitives.ReadUInt64LittleEndian(span.Slice(60)) == 0 && BinaryPrimitives.ReadUInt64LittleEndian(span.Slice(68)) == 0)
                    {
                        return true;
                    }
                }

                return false;
            }

            return true;
        }
    }
}
