using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;

namespace Wist.Node.Core.Interfaces
{
    [ExtensionPoint]
    public interface IRole
    {
        void Initialize();

        void Start();

        void Stop();
    }
}
