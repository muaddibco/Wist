using System;
using System.Buffers.Binary;
using System.Numerics;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.ExtensionMethods;
using Wist.Core.Logging;
using Wist.Core.ProofOfWork;
using Wist.Core.States;
using Wist.Core.Synchronization;

namespace Wist.BlockLattice.Core.Handlers
{
    [RegisterExtension(typeof(ICoreVerifier), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class SyncHeightVerifier : ICoreVerifier
    {
        private readonly ILogger _log;
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly IProofOfWorkCalculation _proofOfWorkCalculation;

        public SyncHeightVerifier(IStatesRepository statesRepository, IProofOfWorkCalculationRepository proofOfWorkCalculationFactory, ILoggerService loggerService)
        {
            _log = loggerService.GetLogger(GetType().Name);
            _synchronizationContext = statesRepository.GetInstance<SynchronizationContext>();
            _proofOfWorkCalculation = proofOfWorkCalculationFactory.Create(Globals.POW_TYPE);
        }

        public bool VerifyBlock(BlockBase blockBase)
        {
            SyncedBlockBase syncedBlockBase = (SyncedBlockBase)blockBase;

            ulong syncBlockHeight = syncedBlockBase.SyncBlockHeight;

            if (!((_synchronizationContext.LastBlockDescriptor?.BlockHeight.Equals(syncBlockHeight) ?? true) ||
                (_synchronizationContext.PrevBlockDescriptor?.BlockHeight.Equals(syncBlockHeight) ?? true)))
            {
                _log.Error($"Synchronization block height is outdated: {blockBase.RawData.ToHexString()}");
                return false;
            }

            return CheckSyncPOW(syncedBlockBase);
        }

        private bool CheckSyncPOW(SyncedBlockBase syncedBlockBase)
        {
            ulong syncBlockHeight = syncedBlockBase.SyncBlockHeight;
            uint nonce = syncedBlockBase.Nonce;
            byte[] powHash = syncedBlockBase.HashNonce;

            //TODO: make difficulty check dynamic
            if (powHash[0] != 0 && powHash[1] != 0)
                return false;

            BigInteger bigInteger = new BigInteger((syncBlockHeight == _synchronizationContext.LastBlockDescriptor.BlockHeight) ? _synchronizationContext.LastBlockDescriptor.Hash : _synchronizationContext.PrevBlockDescriptor.Hash);
            bigInteger += nonce;

            byte[] input = bigInteger.ToByteArray();
            byte[] computedHash = _proofOfWorkCalculation.CalculateHash(input);

            if (!computedHash.Equals24(powHash))
            {
                _log.Error("Computed HASH differs from obtained one");
                return false;
            }

            return true;
        }
    }
}
