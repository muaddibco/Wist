using System;

namespace Wist.Core.PerformanceCounters
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class PerfCounterCategoryAttribute : Attribute
    {
        static PerfCounterCategoryAttribute()
        {
            Type t = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType;
        }

        public PerfCounterCategoryAttribute(string name, string help = null, string version = "1.0.0.0")
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            try
            {
                Version.Parse(version);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Failed to parse version expression {version}", e);
            }

            Name = name + "." + version;
            Help = help;
        }

        public string Name { get; }
        public string Help { get; }
    }
}
