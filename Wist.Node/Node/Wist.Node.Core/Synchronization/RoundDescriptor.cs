using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Timers;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.Core.Identity;

namespace Wist.Node.Core.Synchronization
{
    public class RoundDescriptor
    {
        private readonly IIdentityKeyProvider _identityKeyProvider;

        private readonly Action<RoundDescriptor> _callbackAction;
        private readonly int _dueTime;
        private readonly object _sync = new object();

        private Timer _timer;

        public RoundDescriptor(IIdentityKeyProvider identityKeyProvider, Action<RoundDescriptor> callbackAction, int dueTime)
        {
            _callbackAction = callbackAction;
            _dueTime = dueTime;
            CandidateBlocks = new Dictionary<IKey, RegistryFullBlock>();
            CandidateVotes = new Dictionary<IKey, int>();
            VotingBlocks = new HashSet<RegistryConfidenceBlock>();
            _identityKeyProvider = identityKeyProvider;
        }
        
        public Dictionary<IKey, RegistryFullBlock> CandidateBlocks { get; }
        public Dictionary<IKey, int> CandidateVotes { get; }
        public HashSet<RegistryConfidenceBlock> VotingBlocks { get; }
        public bool IsFinished { get; set; }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetTimer()
        {
            if (_timer == null)
            {
                lock (_sync)
                {
                    if (_timer == null)
                    {
                        _timer = new Timer(_dueTime) { AutoReset = false };
                        _timer.Elapsed += _timer_Elapsed;
                        _timer.Start();
                    }
                }
            }
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e) => _callbackAction?.Invoke(this);

        private void TimerCallback(object state)
        {
            _callbackAction?.Invoke(this);
        }

        public void AddFullBlock(RegistryFullBlock registryFullBlock)
        {
            SetTimer();
            
            IKey key = _identityKeyProvider.GetKey(registryFullBlock.ShortBlockHash);
            if (!CandidateBlocks.ContainsKey(key))
            {
                CandidateBlocks.Add(key, registryFullBlock);
                CandidateVotes.Add(key, 0);
            }
        }

        public void AddVotingBlock(RegistryConfidenceBlock registryConfidenceBlock)
        {
            SetTimer();

            VotingBlocks.Add(registryConfidenceBlock);
        }

        public void Reset()
        {
            IsFinished = false;
            CandidateBlocks.Clear();
            CandidateVotes.Clear();
            VotingBlocks.Clear();
            _timer.Stop();
            _timer.Dispose();
            _timer = null;
        }
    }
}
