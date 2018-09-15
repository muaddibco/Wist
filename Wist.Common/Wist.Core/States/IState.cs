using System;
using System.Threading.Tasks.Dataflow;
using Wist.Core.Architecture;

namespace Wist.Core.States
{
    [ExtensionPoint]
    public interface IState
    {
        string Name { get; }

        IDisposable SubscribeOnStateChange(ITargetBlock<string> targetBlock);
    }
}
