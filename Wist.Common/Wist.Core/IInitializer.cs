using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Core
{
    [ExtensionPoint]
    public interface IInitializer
    {
        ExtensionOrderPriorities Priority { get; }

        bool Initialized { get; }

        void Initialize(CancellationToken cancellationToken);
    }
}
