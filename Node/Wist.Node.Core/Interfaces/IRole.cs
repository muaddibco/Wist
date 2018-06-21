using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;

namespace Wist.Node.Core.Interfaces
{
    [ExtensionPoint]
    public interface IRole : IEquatable<IRole>
    {
        string Name { get; }

        void Initialize();

        void Start();

        void Stop();
    }
}
