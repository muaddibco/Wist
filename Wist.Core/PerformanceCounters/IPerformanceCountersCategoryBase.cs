using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;

namespace Wist.Core.PerformanceCounters
{
    [ExtensionPoint]
    public interface IPerformanceCountersCategoryBase
    {
        string Name { get; }

        void Initialize();

        void Setup();
    }
}
