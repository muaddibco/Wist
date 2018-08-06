using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Wist.Core.PerformanceCounters
{
    [CounterType(CounterType = PerformanceCounterType.AverageTimer32)]
    public class TimespanCounter : PerformanceCounterBase
    {
        /// <summary>
        /// Used for representation of a timestamp by the performance counters.
        /// </summary>
        private long _frequency;

        //public TimespanCounter(string categoryName, string counterName, string instanceName) :
        //    base(categoryName, counterName, instanceName, PerformanceCounterType.AverageTimer32)
        //{
        //}

        protected AverageBaseCounter baseCounter { get; set; }

        /// <summary>
        /// Queries the frequency used for representation of a timestamp by the performance counters.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        static extern short QueryPerformanceFrequency(ref long x);

        public override void Initialize(string categoryName, string counterName, string instanceName, PerformanceCounterType expectedType)
        {
            base.Initialize(categoryName, counterName, instanceName, expectedType);

            QueryPerformanceFrequency(ref _frequency);
            baseCounter = new AverageBaseCounter();
            baseCounter.Initialize(categoryName, CategoryFactory.GetBaseNameFromCounter(counterName), instanceName, PerformanceCounterType.AverageBase);
        }

        public TimeSpan IncrementBy(TimeSpan difference)
        {
            _counter.IncrementBy((long)difference.TotalMilliseconds * _frequency / 1000);
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
