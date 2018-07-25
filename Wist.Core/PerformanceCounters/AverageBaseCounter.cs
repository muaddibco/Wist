﻿using System.Diagnostics;

namespace Wist.Core.PerformanceCounters
{
    [CounterType(CounterType = PerformanceCounterType.AverageBase)]
    public class AverageBaseCounter : PerformanceCounterBase, IPerformanceCounter<long>
    {
        public AverageBaseCounter(string categoryName, string counterName, string instanceName, bool readOnly = false) :
            base(categoryName, counterName, instanceName, PerformanceCounterType.AverageBase, readOnly)
        {
        }

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
