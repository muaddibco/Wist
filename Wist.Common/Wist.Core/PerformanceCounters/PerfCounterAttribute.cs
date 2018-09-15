using System;

namespace Wist.Core.PerformanceCounters
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public sealed class PerfCounterAttribute : Attribute
    {
        public PerfCounterAttribute(string name, string help)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            Name = string.IsNullOrEmpty(name) ? string.Empty : name;
        }

        public string Name { get; }

        public override string ToString()
        {
            return $"{GetType().Name}, name = {Name}";
        }
    }
}
