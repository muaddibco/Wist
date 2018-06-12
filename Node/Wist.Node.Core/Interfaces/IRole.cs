using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Wist.Core.Architecture;

namespace Wist.Node.Core.Interfaces
{
    [ExtensionPoint]
    public interface IRole
    {
        string Name { get; }

        void Initialize();

        Task Play();
    }
}
