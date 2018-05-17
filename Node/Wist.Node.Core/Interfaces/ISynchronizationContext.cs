﻿using System;
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
        SynchronizationConfirmedBlock LastSyncBlock { get; set; }

        DateTime LastSyncBlockReceivingTime { get; set; }

        List<ConsensusGroupParticipant> Participants { get; set; }
    }
}
