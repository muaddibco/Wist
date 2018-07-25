using System;
using System.Diagnostics;

namespace Wist.Core.PerformanceCounters
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public sealed class CounterTypeAttribute : Attribute
    {
        public CounterTypeAttribute() { }

        public PerformanceCounterType CounterType { get; set; }
    }
}
