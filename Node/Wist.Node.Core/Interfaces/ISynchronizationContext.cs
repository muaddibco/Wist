using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel.Synchronization;
using Wist.Core.Architecture;
using Wist.Node.Core.Model;

namespace Wist.Node.Core.Interfaces
{
    [ServiceContract]
    public interface ISynchronizationContext
    {
        /// <summary>
        /// Last synchronization block obtained from Network
        /// </summary>
        SynchronizationConfirmedBlock LastSyncBlock { get; set; }

        /// <summary>
        /// Local date and time when last synchronization block was obtained
        /// </summary>
        DateTime LastSyncBlockReceivingTime { get; set; }

        /// <summary>
        /// Complete list of current participants involved into producing synchronization blocks
        /// </summary>
        List<ConsensusGroupParticipant> Participants { get; set; }

        /// <summary>
        /// Utility function that returns median value from provided array
        /// </summary>
        /// <param name="dateTimes"></param>
        /// <returns></returns>
        DateTime GetMedianValue(IEnumerable<DateTime> dateTimes);
    }
}
