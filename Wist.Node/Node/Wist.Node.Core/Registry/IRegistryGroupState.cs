using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Communication;
using Wist.Core.Identity;

namespace Wist.Node.Core.Registry
{
    public interface IRegistryGroupState : INeighborhoodState
    {
        IKey SyncLayerNode { get; set; }

        int Round { get; set; }

        void ToggleLastBlockConfirmationReceived();

        void WaitLastBlockConfirmationReceived();
    }
}
