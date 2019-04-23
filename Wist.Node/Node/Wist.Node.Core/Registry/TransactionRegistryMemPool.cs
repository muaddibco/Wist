using System.Collections.Generic;
using System.Linq;
using Wist.Blockchain.Core.DataModel.Registry;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.ExtensionMethods;
using Wist.Core.Identity;
using Wist.Core.Logging;
using Wist.Core.States;
using Wist.Core.Synchronization;
using Wist.Blockchain.Core;
using Wist.Core.HashCalculations;
using Wist.Core;

namespace Wist.Node.Core.Registry
{
    //TODO: add performance counter for measuring MemPool size

    /// <summary>
    /// MemPool is needed for following purposes:
    ///  1. Source for building transactions registry block
    ///  2. Repository for comparing transactions registry key arrived with transactions registry block from another participant
    ///  
    ///  When created Transaction Registry Block gets approved by corresponding node from Sync layer transaction enumerated there must be removed from the Pool
    /// </summary>
    [RegisterDefaultImplementation(typeof(IRegistryMemPool), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionRegistryMemPool : IRegistryMemPool
    {
        private readonly List<RegistryRegisterBlock> _transactionStateWitnesses;
        private readonly List<RegistryRegisterUtxoConfidential> _transactionUtxoWitnesses;
        private readonly Dictionary<IKey, List<RegistryRegisterBlock>> _transactionStateWitnessesBySender;
        private readonly Dictionary<IKey, RegistryRegisterUtxoConfidential> _transactionUtxoWitnessesByKeyImage;

        private readonly Dictionary<ulong, Dictionary<ulong, HashSet<RegistryShortBlock>>> _transactionsShortBlocks;
        private readonly IIdentityKeyProvider _transactionHashKey;
        private readonly ILogger _logger;
        //private readonly Timer _timer;
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly IHashCalculation _hashCalculation;
        private int _oldValue;
        private readonly object _sync = new object();

        public TransactionRegistryMemPool(ILoggerService loggerService, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, 
            IStatesRepository statesRepository, IHashCalculationsRepository hashCalculationsRepository)
        {
            _oldValue = 0;

            _transactionHashKey = identityKeyProvidersRegistry.GetTransactionsIdenityKeyProvider();
            _logger = loggerService.GetLogger(nameof(TransactionRegistryMemPool));

            _transactionStateWitnesses = new List<RegistryRegisterBlock>();
            _transactionUtxoWitnesses = new List<RegistryRegisterUtxoConfidential>();
            _transactionStateWitnessesBySender = new Dictionary<IKey, List<RegistryRegisterBlock>>();
            _transactionUtxoWitnessesByKeyImage = new Dictionary<IKey, RegistryRegisterUtxoConfidential>();

            _transactionsShortBlocks = new Dictionary<ulong, Dictionary<ulong, HashSet<RegistryShortBlock>>>();
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
            _hashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
        }

        public bool EnqueueTransactionWitness(RegistryRegisterBlock transactionWitness)
        {
            lock (_sync)
            {
                bool witnessExist = false;

                if(_transactionStateWitnessesBySender.ContainsKey(transactionWitness.Signer))
                {
                    witnessExist = _transactionStateWitnessesBySender[transactionWitness.Signer].Any(t => t.BlockHeight == transactionWitness.BlockHeight);
                }

                if(!witnessExist)
                {
                    if (!_transactionStateWitnessesBySender.ContainsKey(transactionWitness.Signer))
                    {
                        _transactionStateWitnessesBySender.Add(transactionWitness.Signer, new List<RegistryRegisterBlock>());
                    }

                    _transactionStateWitnessesBySender[transactionWitness.Signer].Add(transactionWitness);
                    _transactionStateWitnesses.Add(transactionWitness);
                }

                return witnessExist;
            }
        }

        public bool EnqueueTransactionWitness(RegistryRegisterUtxoConfidential transactionWitness)
        {
            lock (_sync)
            {
                if (!_transactionUtxoWitnessesByKeyImage.ContainsKey(transactionWitness.KeyImage))
                {
                    _transactionUtxoWitnessesByKeyImage.Add(transactionWitness.KeyImage, transactionWitness);

                    _transactionUtxoWitnesses.Add(transactionWitness);

                    return true;
                }
            }

            return false;
        }

        public bool EnqueueTransactionsShortBlock(RegistryShortBlock transactionsShortBlock)
        {
            lock (_sync)
            {
                if (!_transactionsShortBlocks[transactionsShortBlock.SyncBlockHeight].ContainsKey(transactionsShortBlock.BlockHeight))
                {
                    _transactionsShortBlocks[transactionsShortBlock.SyncBlockHeight].Add(transactionsShortBlock.BlockHeight, new HashSet<RegistryShortBlock>());
                }

                return _transactionsShortBlocks[transactionsShortBlock.SyncBlockHeight][transactionsShortBlock.BlockHeight].Add(transactionsShortBlock);
            }
        }

        public void ClearWitnessed(RegistryShortBlock transactionsShortBlock)
        {
            lock (_sync)
            {
                foreach (var witnessStateKey in transactionsShortBlock.WitnessStateKeys)
                {
                    if(_transactionStateWitnessesBySender.ContainsKey(witnessStateKey.PublicKey))
                    {
                        RegistryRegisterBlock transactionWitness = _transactionStateWitnessesBySender[witnessStateKey.PublicKey].FirstOrDefault(t => t.BlockHeight == witnessStateKey.Height);

                        if(transactionWitness != null)
                        {
                            _transactionStateWitnessesBySender[witnessStateKey.PublicKey].Remove(transactionWitness);
                            if(_transactionStateWitnessesBySender[witnessStateKey.PublicKey].Count == 0)
                            {
                                _transactionStateWitnessesBySender.Remove(witnessStateKey.PublicKey);
                            }

                            _transactionStateWitnesses.Remove(transactionWitness);
                        }
                    }
                }

                foreach (var witnessUtxoKey in transactionsShortBlock.WitnessUtxoKeys)
                {
                    if (_transactionUtxoWitnessesByKeyImage.ContainsKey(witnessUtxoKey.KeyImage))
                    {
                        RegistryRegisterUtxoConfidential transactionWitness = _transactionUtxoWitnessesByKeyImage[witnessUtxoKey.KeyImage];

                        _transactionStateWitnessesBySender.Remove(witnessUtxoKey.KeyImage);
                        _transactionUtxoWitnesses.Remove(transactionWitness);
                    }
                }
            }
        }

        //TODO: need to understand whether it is needed to pass height of Sync Block or automatically take latest one?
        public SortedList<ushort, RegistryRegisterBlock> DequeueStateWitnessBulk()
        {
            SortedList<ushort, RegistryRegisterBlock> items = new SortedList<ushort, RegistryRegisterBlock>();
            lock (_sync)
            {
                ushort order = 0;

                foreach (var transactionWitness in _transactionStateWitnesses)
                {
                    items.Add(order++, transactionWitness);

                    if (order == ushort.MaxValue)
                    {
                        break;
                    }
                }
            }

            _logger.Debug($"MemPool returns {items.Count} State Witness items");
            return items;
        }
        public SortedList<ushort, RegistryRegisterUtxoConfidential> DequeueUtxoWitnessBulk()
        {
            SortedList<ushort, RegistryRegisterUtxoConfidential> items = new SortedList<ushort, RegistryRegisterUtxoConfidential>();
            lock (_sync)
            {
                ushort order = 0;

                foreach (var transactionWitness in _transactionUtxoWitnesses)
                {
                    items.Add(order++, transactionWitness);

                    if (order == ushort.MaxValue)
                    {
                        break;
                    }
                }
            }

            _logger.Debug($"MemPool returns {items.Count} UTXO Witness items");
            return items;
        }

        public RegistryShortBlock GetRegistryShortBlockByHash(ulong syncBlockHeight, ulong round, byte[] hash)
        {
            if (!_transactionsShortBlocks.ContainsKey(syncBlockHeight))
            {
                return null;
            }

            if(!_transactionsShortBlocks[syncBlockHeight].ContainsKey(round))
            {
                return null;
            }

            RegistryShortBlock registryShortBlock = _transactionsShortBlocks[syncBlockHeight][round].FirstOrDefault(s => _hashCalculation.CalculateHash(s.RawData).Equals32(hash));

            return registryShortBlock;
        }
    }
}
