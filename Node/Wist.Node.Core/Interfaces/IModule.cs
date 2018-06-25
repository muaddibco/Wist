using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Wist.Core.Architecture;
using Wist.Core.Aspects;

namespace Wist.Node.Core.Interfaces
{
    [ExtensionPoint]
    public interface IModule
    {
        bool IsInitialized { get; }

        string Name { get; }

        void Initialize();
    }
}
