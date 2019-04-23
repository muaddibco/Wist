using System;
using System.Buffers.Binary;
using Wist.Blockchain.Core;
using Wist.Blockchain.Core.DataModel;
using Wist.Blockchain.Core.Enums;
using Wist.Blockchain.Core.Handlers;
using Wist.Blockchain.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.ExtensionMethods;
using Wist.Core.Logging;
using Wist.Core.HashCalculations;
using Wist.Core.States;
using Wist.Core.Synchronization;
using Wist.Core.Models;
using Wist.Core;

namespace Wist.Node.Core.Synchronization
{
    [RegisterExtension(typeof(IPacketVerifier), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationConfirmedVerifier : IPacketVerifier
    {
        private readonly ISynchronizationContext _synchronizationContext;
		private readonly ILoggerService _loggerService;

		public SynchronizationConfirmedVerifier(IStatesRepository statesRepository, ILoggerService loggerService) 
        {
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
			_loggerService = loggerService;
		}

        public PacketType PacketType => PacketType.Synchronization;

        public bool ValidatePacket(PacketBase blockBase)
        {
            LinkedPacketBase syncedBlockBase = (LinkedPacketBase)blockBase;

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
