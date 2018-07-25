using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Wist.Core.PerformanceCounters
{
    [CounterType(CounterType = PerformanceCounterType.AverageTimer32)]
    public class TimespanCounter : PerformanceCounterBase, IPerformanceCounterBase<TimeSpan>
    {
        public TimespanCounter(string categoryName, string counterName, string instanceName) :
            base(categoryName, counterName, instanceName, PerformanceCounterType.AverageTimer32)
        {
            QueryPerformanceFrequency(ref frequency);
            baseCounter = new AverageBaseCounter(categoryName, CategoryFactory.GetBaseNameFromCounter(counterName), instanceName);
        }

        protected AverageBaseCounter baseCounter { get; set; }

        /// <summary>
        /// Queries the frequency used for representation of a timestamp by the performance counters.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        static extern short QueryPerformanceFrequency(ref long x);

        /// <summary>
        /// Used for representation of a timestamp by the performance counters.
        /// </summary>
        private readonly long frequency;


        public TimeSpan IncrementBy(TimeSpan difference)
        {
            _counter.IncrementBy((long)difference.TotalMilliseconds * frequency / 1000);
            baseCounter.Increment();
            return difference;
        }

        public TimeSpan IncrementBy(DateTime start)
        {
            var difference = DateTime.Now - start;
            return IncrementBy(difference);
        }

        public TimeSpan UtcIncrementBy(DateTime start)
        {
            var difference = DateTime.UtcNow - start;
            return IncrementBy(difference);
        }
    }
}
