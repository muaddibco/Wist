using System;

namespace Wist.Client.Common.Communication
{
    public interface INetworkSynchronizer
    {
        DateTime LastSyncTime { get; set; }
    }
}