﻿using System.Diagnostics;

namespace Wist.Core.PerformanceCounters
{
    /// <summary>
    /// A concrete implementation for an Integer value type counter over Microsoft library.
    /// </summary>
    [CounterType(CounterType = PerformanceCounterType.AverageCount64)]
    public class SuccessRateCounter : PerformanceCounterBase
    {
        public SuccessRateCounter(string categoryName, string counterName, string instanceName) :
            base(categoryName, counterName, instanceName, PerformanceCounterType.AverageCount64, false)
        {
            baseCounter = new AverageBaseCounter(categoryName, CategoryFactory.GetBaseNameFromCounter(counterName), instanceName);
        }

        public long Success()
        {
            baseCounter.Increment();
            return _counter.IncrementBy(100);
        }

        public long Failure()
        {
            baseCounter.Increment();
            return _counter.IncrementBy(0);
        }

        protected AverageBaseCounter baseCounter { get; set; }
    }
}
