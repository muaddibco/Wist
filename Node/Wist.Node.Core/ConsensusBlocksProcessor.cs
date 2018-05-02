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

        public ConsensusBlocksProcessor()
        {
            _blocks = new BlockingCollection<BlockBase>();
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
    }
}
