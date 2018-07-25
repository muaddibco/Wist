using System.Diagnostics;

namespace Wist.Core.PerformanceCounters
{
    /// <summary>
    /// A concrete implementation for an Integer value type counter over Microsoft library.
    /// </summary>
    [CounterType(CounterType = PerformanceCounterType.AverageCount64)]
    public class AverageCountCounter : PerformanceCounterBase
    {
        //public AverageCountCounter(string categoryName, string counterName, string instanceName) :
        //    base(categoryName, counterName, instanceName, PerformanceCounterType.AverageCount64, false)
        //{
        //    baseCounter = new AverageBaseCounter(categoryName, CategoryFactory.GetBaseNameFromCounter(counterName), instanceName);
        //}

        public override void Initialize(string categoryName, string counterName, string instanceName, PerformanceCounterType expectedType)
        {
            baseCounter = new AverageBaseCounter();
            baseCounter.Initialize(categoryName, CategoryFactory.GetBaseNameFromCounter(counterName), instanceName, PerformanceCounterType.AverageBase);

            base.Initialize(categoryName, counterName, instanceName, expectedType);
        }

        public long IncrementBy(long value)
        {
            baseCounter.Increment();
            return _counter.IncrementBy(value);
        }

        public long Increment()
        {
            baseCounter.Increment();
            return _counter.Increment();
        }

        public long Decrement()
        {
            baseCounter.Decrement();
            return _counter.Decrement();
        }

        protected AverageBaseCounter baseCounter { get; set; }
    }
}
