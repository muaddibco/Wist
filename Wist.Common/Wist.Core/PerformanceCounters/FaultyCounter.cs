using System.Diagnostics;

namespace Wist.Core.PerformanceCounters
{
    public class FaultyCounter : PerformanceCounterBase
    {
        //public FaultyCounter(string categoryName, string counterName, string instanceName) :
        //    base(categoryName, counterName, instanceName, PerformanceCounterType.NumberOfItems64)
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
            return _counter.IncrementBy((long)value);
        }
    }
}
