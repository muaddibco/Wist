using Wist.Communication.Sockets;
using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Interfaces;
using System.Threading.Tasks;
using Wist.Core.Models;

namespace Wist.Communication.Interfaces
{
    [ServiceContract]
    public interface ICommunicationService
    {
        void Init(SocketListenerSettings settings, IBlocksProcessor blocksProcessor, ICommunicationProvisioning communicationProvisioning = null);

        void StartListen();

        void PostMessage(IKey destination, IMessage message);
        void PostMessage(IEnumerable<IKey> destinations, IMessage message);
    }
}
