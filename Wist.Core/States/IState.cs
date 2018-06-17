using System.Threading.Tasks.Dataflow;
using Wist.Core.Architecture;

namespace Wist.Core.States
{
    [ExtensionPoint]
    public interface IState
    {
        string Name { get; }

        void SubscribeOnStateChange(ITargetBlock<string> targetBlock);
    }
}
