using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.ExtensionMethods;

namespace Wist.Core.Synchronization
{
    public class SynchronizationDescriptor
    {
        public SynchronizationDescriptor(ulong blockHeight, byte[] hash, DateTime medianTime, DateTime updateTime, ushort round)
        {
            BlockHeight = blockHeight;
            Hash = hash;
            MedianTime = medianTime;
            UpdateTime = updateTime;
            Round = round;
        }

        /// <summary>
        /// Last synchronization block obtained from Network
        /// </summary>
        public ulong BlockHeight { get; private set; }

        public ushort Round { get; set; }

        public byte[] Hash { get; private set; }

        public DateTime MedianTime { get; private set; }

        /// <summary>
        /// Local date and time when last synchronization block was obtained
        /// </summary>
        public DateTime UpdateTime { get; private set; }

        public override string ToString()
        {
            return $"[{BlockHeight} @ {UpdateTime}]: {MedianTime}; Hash = {Hash.ToHexString()}";
        }
    }
}
