using System.Diagnostics;

namespace Wist.Core.PerformanceCounters
{
    //TODO: need to initialize BaseRawValue
    [CounterType(CounterType = PerformanceCounterType.RawBase)]
    public class RawBaseCounter : PerformanceCounterBase
    {
        //public RawBaseCounter(string categoryName, string counterName, string instanceName, long BaseRawValue, bool readOnly = false) :
        //    base(categoryName, counterName, instanceName, PerformanceCounterType.RawBase, readOnly)
        //{
        //    RawValue = BaseRawValue;
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
