using System;
using System.Collections.Generic;
using Wist.Blockchain.Core.DataModel.Registry;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Logging;
using System.Collections.ObjectModel;
using Wist.Core.HashCalculations;
using Wist.Core.ExtensionMethods;
using Wist.Blockchain.Core;
using System.Linq;
using Wist.Blockchain.Core.DataModel.Synchronization;
using Wist.Core;

namespace Wist.Node.Core.Synchronization
{
    [RegisterDefaultImplementation(typeof(ISyncRegistryMemPool), Lifetime = LifetimeManagement.Singleton)]
    public class SyncRegistryMemPool : ISyncRegistryMemPool
    {
        private readonly int _maxCombinedBlocks = 30;
        private readonly object _syncRound = new object();
        private readonly List<RegistryFullBlock> _registryBlocks = new List<RegistryFullBlock>();
        private readonly List<SynchronizationRegistryCombinedBlock> _registryCombinedBlocks = new List<SynchronizationRegistryCombinedBlock>();
        private readonly IHashCalculation _defaultTransactionHashCalculation;
        private readonly ILogger _logger;

        public SyncRegistryMemPool(ILoggerService loggerService, IHashCalculationsRepository hashCalculationsRepository)
        {
            _logger = loggerService.GetLogger(nameof(SyncRegistryMemPool));
            _defaultTransactionHashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
        }

        public void AddCandidateBlock(RegistryFullBlock transactionsFullBlock)
        {
            if (transactionsFullBlock == null)
            {
                throw new ArgumentNullException(nameof(transactionsFullBlock));
            }

            _logger.Debug($"Adding candidate block of round {transactionsFullBlock.BlockHeight} with {transactionsFullBlock.StateWitnesses.Length + transactionsFullBlock.UtxoWitnesses.Length} transactions");

            byte[] hash = _defaultTransactionHashCalculation.CalculateHash(transactionsFullBlock.RawData);

            if(_registryCombinedBlocks.Any(b => b.BlockHashes.Any(h => h.Equals32(hash))))
            {
                return;
            }

            lock (_registryBlocks)
            {
                _registryBlocks.Add(transactionsFullBlock);
            }
        }


        public IEnumerable<RegistryFullBlock> GetRegistryBlocks()
        {
            lock(_registryBlocks)
            {
                List<RegistryFullBlock> blocks = new List<RegistryFullBlock>(_registryBlocks);
                ReadOnlyCollection<RegistryFullBlock> registryFullBlocks = new ReadOnlyCollection<RegistryFullBlock>(blocks);

                _registryBlocks.Clear();

                return registryFullBlocks;
            }
        }

        public void RegisterCombinedBlock(SynchronizationRegistryCombinedBlock combinedBlock)
        {
            List<SynchronizationRegistryCombinedBlock> toRemove = _registryCombinedBlocks.Where(b => (int)(combinedBlock.BlockHeight - b.BlockHeight) > _maxCombinedBlocks).ToList();

            foreach (var item in toRemove)
            {
                _registryCombinedBlocks.Remove(item);
            }

            _registryCombinedBlocks.Add(combinedBlock);
            
            RemoveRange(combinedBlock.BlockHashes);
        }

        #region Private Functions

        private static long NumberOfSetBits(long i)
        {
            i = i - ((i >> 1) & 0x5555555555555555);
            i = (i & 0x3333333333333333) + ((i >> 2) & 0x3333333333333333);
            return (((i + (i >> 4)) & 0xF0F0F0F0F0F0F0F) * 0x101010101010101) >> 56;
        }

        private void RemoveRange(IEnumerable<byte[]> registryFullBlockHashes)
        {
            lock (_registryBlocks)
            {
                List<RegistryFullBlock> toRemove = _registryBlocks.Where(b => registryFullBlockHashes.Any(h => _defaultTransactionHashCalculation.CalculateHash(b.RawData).Equals32(h))).ToList();

                foreach (var item in toRemove)
                {
                    _registryBlocks.Remove(item);
                }
            }
        }

        #endregion Private Functions
    }
}
