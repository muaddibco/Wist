using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Core.Synchronization
{
    public class SynchronizationDescriptor
    {
        /// <summary>
        /// Last synchronization block obtained from Network
        /// </summary>
        public uint BlockHeight { get; set; }

        public byte[] Hash { get; set; }

        public DateTime MedianTime { get; set; }

        /// <summary>
        /// Local date and time when last synchronization block was obtained
        /// </summary>
        public DateTime ReceivingTime { get; set; }
    }
}
