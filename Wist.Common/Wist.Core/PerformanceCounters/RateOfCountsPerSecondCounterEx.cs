using System.Diagnostics;

namespace Wist.Core.PerformanceCounters
{
    /// <summary>
    /// A concrete implementation for an Integer value type counter over Microsoft library.
    /// </summary>
    [CounterType(CounterType = PerformanceCounterType.RateOfCountsPerSecond64)]
    public class RateOfCountsPerSecondCounterEx : PerformanceCounterBase
    {
        //public RateOfCountsPerSecondCounterEx(string categoryName, string counterName, string instanceName, bool readOnly = false) :
        //    base(categoryName, counterName, instanceName, PerformanceCounterType.RateOfCountsPerSecond64, readOnly)
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
