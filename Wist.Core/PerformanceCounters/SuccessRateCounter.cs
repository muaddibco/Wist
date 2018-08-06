using System.Diagnostics;

namespace Wist.Core.PerformanceCounters
{
    /// <summary>
    /// A concrete implementation for an Integer value type counter over Microsoft library.
    /// </summary>
    [CounterType(CounterType = PerformanceCounterType.AverageCount64)]
    public class SuccessRateCounter : PerformanceCounterBase
    {
        //public SuccessRateCounter(string categoryName, string counterName, string instanceName) :
        //    base(categoryName, counterName, instanceName, PerformanceCounterType.AverageCount64, false)
        //{
        //    baseCounter = new AverageBaseCounter(categoryName, CategoryFactory.GetBaseNameFromCounter(counterName), instanceName);
        //}

        public override void Initialize(string categoryName, string counterName, string instanceName, PerformanceCounterType expectedType)
        {
            base.Initialize(categoryName, counterName, instanceName, expectedType);

            BaseCounter = new AverageBaseCounter();
            BaseCounter.Initialize(categoryName, CategoryFactory.GetBaseNameFromCounter(counterName), instanceName, PerformanceCounterType.AverageBase);
        }

        public long Success()
        {
            BaseCounter.Increment();
            return _counter.IncrementBy(100);
        }

        public long Failure()
        {
            BaseCounter.Increment();
            return _counter.IncrementBy(0);
        }

        protected AverageBaseCounter BaseCounter { get; set; }
    }
}
