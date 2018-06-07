using Wist.Communication.Sockets;
using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Interfaces;
using System.Threading.Tasks;

namespace Wist.Communication.Interfaces
{
    [ServiceContract]
    public interface ICommunicationServer
    {
        void Init(SocketListenerSettings settings, IBlocksProcessor blocksProcessor, ICommunicationProvisioning communicationProvisioning = null);

        void StartListen();

        Task BroadcastMessage(BlockBase message);
    }
}
