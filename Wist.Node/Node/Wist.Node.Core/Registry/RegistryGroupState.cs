using System.Threading;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Identity;
using Wist.Core.States;

namespace Wist.Node.Core.Registry
{
    [RegisterExtension(typeof(IState), Lifetime = LifetimeManagement.Singleton)]
    public class RegistryGroupState : NeighborhoodStateBase, IRegistryGroupState
    {
        private readonly AutoResetEvent _registrationBlockConfirmationReceived;

        public RegistryGroupState()
        {
            _registrationBlockConfirmationReceived = new AutoResetEvent(false); //TODO: need to be set to true in case when network is before bootstrap stage
        }

        public const string NAME = nameof(IRegistryGroupState);

        public override string Name => NAME;

        public int Round { get; set; }

        public IKey SyncLayerNode { get; set; }

        public void ToggleLastBlockConfirmationReceived() => _registrationBlockConfirmationReceived.Set();
        public void WaitLastBlockConfirmationReceived() => _registrationBlockConfirmationReceived.WaitOne();
    }
}
