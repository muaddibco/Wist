using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Core.Synchronization
{
    public class SynchronizationDescriptor
    {
        public SynchronizationDescriptor(ulong blockHeight, byte[] hash, DateTime medianTime, DateTime receiveTime)
        {
            BlockHeight = blockHeight;
            Hash = hash;
            MedianTime = medianTime;
            ReceivingTime = receiveTime;
        }

        /// <summary>
        /// Last synchronization block obtained from Network
        /// </summary>
        public ulong BlockHeight { get; private set; }

        public byte[] Hash { get; private set; }

        public DateTime MedianTime { get; private set; }

        /// <summary>
        /// Local date and time when last synchronization block was obtained
        /// </summary>
        public DateTime ReceivingTime { get; private set; }
    }
}
