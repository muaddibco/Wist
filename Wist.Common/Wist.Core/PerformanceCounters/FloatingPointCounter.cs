using System.Diagnostics;

namespace Wist.Core.PerformanceCounters
{
    /// <summary>
    /// Floating point counter with precision of 3 digits after integer part in performance counter.
    /// </summary>
    [CounterType(CounterType = PerformanceCounterType.RawFraction)]
    public class FloatingPointCounter : PerformanceCounterBase
    {
        //public FloatingPointCounter(string categoryName, string counterName, string instanceName) :
        //    base(categoryName, counterName, instanceName, PerformanceCounterType.RawFraction)
        //{
        //    baseCounter = new RawBaseCounter(categoryName, CategoryFactory.GetBaseNameFromCounter(counterName), instanceName, BaseRawValue * 100);
        //}

        public static long BaseRawValue = 1000;

        protected RawBaseCounter baseCounter { get; set; }

        public double IncrementBy(double val)
        {
            long baseVal = (long)(val * BaseRawValue);
            var rc = _counter.IncrementBy(baseVal);
            return (double)rc / BaseRawValue;
        }
    }
}
