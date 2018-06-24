using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;

namespace Wist.Core
{
    [ExtensionPoint]
    public interface IInitializer
    {
        bool Initialized { get; }

        void Initialize();
    }
}
