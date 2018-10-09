using Grpc.Core;
using System;
using System.Net;
using Wist.Core.Architecture;
using Wist.Core.Identity;

namespace Wist.Client.Common.Communication
{
    [ServiceContract]
    public interface INetworkSynchronizer
    {
        DateTime LastSyncTime { get; set; }

        void SendData(IPAddress address, int port, ChannelCredentials channelCredentials, IKey privateKey, IKey targetKey);

        bool ApproveDataSent();
    }
}