using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wist.BlockLattice.Core.DataModel.Synchronization;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Node.Core.Interfaces;
using Wist.Node.Core.Model;

namespace Wist.Node.Core.Synchronization
{
    [RegisterDefaultImplementation(typeof(ISynchronizationContext), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationContext : ISynchronizationContext
    {
        public SynchronizationConfirmedBlock LastSyncBlock { get; set; }
        public DateTime LastSyncBlockReceivingTime { get; set; }
        public List<ConsensusGroupParticipant> Participants { get; set; }


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
    }
}
