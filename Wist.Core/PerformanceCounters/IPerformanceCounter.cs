using System.Diagnostics;

namespace Wist.Core.PerformanceCounters
{
    /// <typeparam name="T">The value type of the counter</typeparam>
    internal interface IPerformanceCounterBase<T>
    {
        long RawValue { get; set; }

        T IncrementBy(T value);

        CounterSample NextSample();

        bool ReadOnly { get; }
    }

    internal interface IPerformanceCounter<T> : IPerformanceCounterBase<T>
    {
        T Increment();
        T Decrement();
    }
}
