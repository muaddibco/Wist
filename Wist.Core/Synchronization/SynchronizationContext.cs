using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Core.Synchronization
{
    [RegisterDefaultImplementation(typeof(ISynchronizationContext), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationContext : ISynchronizationContext
    {
        public SynchronizationDescriptor LastBlockDescriptor { get; set; }

        public SynchronizationDescriptor PrevBlockDescriptor { get; set; }

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
    }
}
