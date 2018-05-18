using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;

namespace Wist.Core.Synchronization
{
    [ServiceContract]
    public interface ISynchronizationContext
    {
        SynchronizationDescriptor LastBlockDescriptor { get; set; }

        SynchronizationDescriptor PrevBlockDescriptor { get; set; }

        /// <summary>
        /// Utility function that returns median value from provided array
        /// </summary>
        /// <param name="dateTimes"></param>
        /// <returns></returns>
        DateTime GetMedianValue(IEnumerable<DateTime> dateTimes);
    }
}
