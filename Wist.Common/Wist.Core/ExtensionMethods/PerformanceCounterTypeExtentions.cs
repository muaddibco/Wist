using System;
using System.Diagnostics;

namespace Wist.Core.ExtensionMethods
{
    public static class PerformanceCounterTypeExtentions
    {
        public static bool IsBaseCounterType(
            this PerformanceCounterType counterType)
        {
            switch (counterType)
            {
                case PerformanceCounterType.AverageBase:
                case PerformanceCounterType.CounterMultiBase:
                case PerformanceCounterType.RawBase:
                case PerformanceCounterType.SampleBase:
                    return true;
                default:
                    return false;
            }
        }

        public static bool HasBaseCounterType(
            this PerformanceCounterType counterType)
        {
            switch (counterType)
            {
                case PerformanceCounterType.AverageTimer32:
                case PerformanceCounterType.AverageCount64:
                case PerformanceCounterType.CounterMultiTimer:
                case PerformanceCounterType.CounterMultiTimer100Ns:
                case PerformanceCounterType.CounterMultiTimerInverse:
                case PerformanceCounterType.CounterMultiTimer100NsInverse:
                case PerformanceCounterType.RawFraction:
                case PerformanceCounterType.SampleFraction:
                case PerformanceCounterType.SampleCounter:
                    return true;
                default:
                    return false;
            }
        }

        public static PerformanceCounterType BaseCounterType(
            this PerformanceCounterType counterType)
        {
            switch (counterType)
            {
                case PerformanceCounterType.AverageTimer32:
                case PerformanceCounterType.AverageCount64:
                    return PerformanceCounterType.AverageBase;
                case PerformanceCounterType.CounterMultiTimer:
                case PerformanceCounterType.CounterMultiTimer100Ns:
                case PerformanceCounterType.CounterMultiTimerInverse:
                case PerformanceCounterType.CounterMultiTimer100NsInverse:
                    return PerformanceCounterType.CounterMultiBase;
                case PerformanceCounterType.RawFraction:
                    return PerformanceCounterType.RawBase;
                case PerformanceCounterType.SampleFraction:
                case PerformanceCounterType.SampleCounter:
                    return PerformanceCounterType.SampleBase;
                default:
                    throw new ArgumentException("");
            }
        }
    }
}
