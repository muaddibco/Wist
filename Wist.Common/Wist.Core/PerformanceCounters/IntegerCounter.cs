using System.Diagnostics;

namespace Wist.Core.PerformanceCounters
{
    [CounterType(CounterType = PerformanceCounterType.NumberOfItems64)]
    public class IntegerCounter : PerformanceCounterBase
    {
        //public IntegerCounter(string categoryName, string counterName, string instanceName, bool readOnly = false) :
        //    base(categoryName, counterName, instanceName, PerformanceCounterType.NumberOfItems64, readOnly)
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

        public long DecrementBy(long value)
        {
            return _counter.RawValue -= value;
        }
    }
}
