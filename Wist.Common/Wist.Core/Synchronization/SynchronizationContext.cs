using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks.Dataflow;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Logging;
using Wist.Core.States;

namespace Wist.Core.Synchronization
{
    [RegisterExtension(typeof(IState), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationContext : ISynchronizationContext
    {
        private readonly Subject<string> _synchronizationSublect;
        private readonly ILogger _logger;

        public SynchronizationContext(ILoggerService loggerService)
        {
            _synchronizationSublect = new Subject<string>();
            _logger = loggerService.GetLogger(nameof(SynchronizationContext));
        }

        public SynchronizationDescriptor LastBlockDescriptor { get; private set; }

        public SynchronizationDescriptor PrevBlockDescriptor { get; private set; }

        public string Name => nameof(ISynchronizationContext);

        public ulong LastRegistrationCombinedBlockHeight { get; set; }

        /// <summary>
        /// Utility function that returns median value from provided array
        /// </summary>
        /// <param name="dateTimes"></param>
        /// <returns></returns>
        public DateTime GetMedianValue(IEnumerable<DateTime> dateTimes)
        {
            IOrderedEnumerable<DateTime> orderedRetransmittedBlocks = dateTimes.OrderBy(v => v);

            int count = orderedRetransmittedBlocks.Count();
            if (count % 2 == 0)
            {
                int indexMidLow = count / 2 - 1;
                int indexMidHigh = count / 2;

                DateTime dtLow = orderedRetransmittedBlocks.ElementAt(indexMidLow);
                DateTime dtHigh = orderedRetransmittedBlocks.ElementAt(indexMidHigh);

                return dtLow.AddSeconds((dtHigh - dtLow).TotalSeconds / 2);
            }
            else
            {
                int index = count / 2;

                return orderedRetransmittedBlocks.ElementAt(index);
            }
        }

        public IDisposable SubscribeOnStateChange(ITargetBlock<string> targetBlock)
        {
            return _synchronizationSublect.Subscribe(targetBlock.AsObserver());
        }

        public void UpdateLastSyncBlockDescriptor(SynchronizationDescriptor synchronizationDescriptor)
        {
            if (synchronizationDescriptor == null)
            {
                throw new ArgumentNullException(nameof(synchronizationDescriptor));
            }

            _logger.Info($"UpdateLastSyncBlockDescriptor: {synchronizationDescriptor}");

            lock (this)
            {

                if (LastBlockDescriptor == null || synchronizationDescriptor.BlockHeight > LastBlockDescriptor.BlockHeight)
                {
                    PrevBlockDescriptor = LastBlockDescriptor;
                    LastBlockDescriptor = synchronizationDescriptor;

                    _synchronizationSublect.OnNext(nameof(LastBlockDescriptor));
                }
            }
        }
    }
}
