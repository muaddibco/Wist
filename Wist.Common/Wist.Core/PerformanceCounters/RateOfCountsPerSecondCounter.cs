using System.Diagnostics;

namespace Wist.Core.PerformanceCounters
{
    [CounterType(CounterType = PerformanceCounterType.RateOfCountsPerSecond32)]
    public class RateOfCountsPerSecondCounter : PerformanceCounterBase
    {
        //public RateOfCountsPerSecondCounter(string categoryName, string counterName, string instanceName, bool readOnly = false) :
        //    base(categoryName, counterName, instanceName, PerformanceCounterType.RateOfCountsPerSecond32, readOnly)
        //{
        //}

        public long Decrement()
        {
            return _counter.Decrement();
        }

        public long Increment()
        {
            return _counter.Increment();
        }

        public long IncrementBy(long value)
        {
            return _counter.IncrementBy(value);
        }
    }
}
