using System;
using System.Buffers.Binary;
using Wist.BlockLattice.Core;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Handlers;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.ExtensionMethods;
using Wist.Core.Logging;
using Wist.Core.HashCalculations;
using Wist.Core.States;
using Wist.Core.Synchronization;

namespace Wist.Node.Core.Synchronization
{
    [RegisterExtension(typeof(IPacketVerifier), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationConfirmedVerifier : IPacketVerifier
    {
        private readonly ISynchronizationContext _synchronizationContext;

        public SynchronizationConfirmedVerifier(IStatesRepository statesRepository, IHashCalculationsRepository proofOfWorkCalculationRepository, ICryptoService cryptoService, ILoggerService loggerService) 
        {
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
        }

        public PacketType PacketType => PacketType.Synchronization;

        public bool ValidatePacket(BlockBase blockBase)
        {
            SyncedLinkedBlockBase syncedBlockBase = (SyncedLinkedBlockBase)blockBase;

            if(syncedBlockBase.BlockType == BlockTypes.Synchronization_ConfirmedBlock && syncedBlockBase.Version == 1)
            {
                if (_synchronizationContext.LastBlockDescriptor != null && _synchronizationContext.LastBlockDescriptor.BlockHeight + 1 <= syncedBlockBase.BlockHeight || _synchronizationContext.LastBlockDescriptor == null)
                {
                    if (_synchronizationContext.LastBlockDescriptor != null && syncedBlockBase.HashPrev.Equals32(_synchronizationContext.LastBlockDescriptor.Hash) ||
                        _synchronizationContext.LastBlockDescriptor == null && syncedBlockBase.HashPrev.Equals32(new byte[Globals.DEFAULT_HASH_SIZE]))
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
