using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Node.Core
{
    [RegisterDefaultImplementation(typeof(IBlocksProcessor), Lifetime = LifetimeManagement.Singleton)]
    public class ConsensusBlocksProcessor : IBlocksProcessor
    {
        private CancellationToken _cancellationToken;

        private readonly object _sync = new object();
        private readonly BlockingCollection<BlockBase> _blocks;
        private bool _isInitialized;

        private readonly List<BlockBase> _verifiedLocally;
        private readonly List<BlockBase> _rejectedLocally;
        private readonly List<BlockBase> _consensusAchievedBlocks;

        public ConsensusBlocksProcessor()
        {
            _blocks = new BlockingCollection<BlockBase>();
            _verifiedLocally = new List<BlockBase>();
            _rejectedLocally = new List<BlockBase>();
            _consensusAchievedBlocks = new List<BlockBase>();
        }

        public void Initialize(CancellationToken ct)
        {
            // TODO: add exception AlreadyInitialized
            if (_isInitialized)
                return;

            lock (_sync)
            {
                if (_isInitialized)
                    return;

                _cancellationToken = ct;

                Task.Factory.StartNew(o =>
                {
                    CancellationToken cancellationToken = (CancellationToken)o;

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        BlockBase blockBase;

                        if (_blocks.TryTake(out blockBase, Timeout.Infinite, cancellationToken))
                        {
                            // Flow will as follows:
                            // 1. Check whether processed block is the only block at the same order received from the same sender?
                            // 2. Check whether processed block is block right after last
                            // 3. Context-dependent data (funds for Transfer and Accept) validation
                            // 4. Once local decision was made it will be retranslated to other nodes in group and their decisions will be accepted
                            // 5. Block, that received majority of votes will be stored to local storage, that means it is correct block
                        }
                    }
                }, _cancellationToken, TaskCreationOptions.LongRunning);

                _isInitialized = true;
            }
        }

        public void ProcessBlock(BlockBase blockBase)
        {
            // TODO: add exception NotInitialized
            if (!_isInitialized)
                return;

            _blocks.Add(blockBase, _cancellationToken);
        }

        #region Private Functions

        private bool CheckBlockUniqueness(BlockBase block)
        {
            bool result = false;

            return result;
        }

        #endregion Private Functions
    }
}
