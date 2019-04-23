using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Wist.Core.Synchronization;

namespace Wist.Node.Core.Registry
{
    internal class SyncCycleDescriptor
    {
        public SyncCycleDescriptor(SynchronizationDescriptor synchronizationDescriptor)
        {
            SynchronizationDescriptor = synchronizationDescriptor;
            CancellationTokenSource = new CancellationTokenSource();
        }

        public SynchronizationDescriptor SynchronizationDescriptor { get; set; }

        public int Round { get; set; }

        public bool CancellationRequested { get; set; }

        public CancellationTokenSource CancellationTokenSource { get; set; }
    }
}
