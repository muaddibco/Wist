﻿using Wist.Communication.Sockets;
using System;
using System.Collections.Generic;
using Wist.Core.Architecture;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Models;
using Wist.Core.Communication;
using System.Net;

namespace Wist.Communication.Interfaces
{
    [ExtensionPoint]
    public interface ICommunicationService
    {
        string Name {get;}
        
        void Init(SocketListenerSettings settings, IBlocksProcessor blocksProcessor, ICommunicationProvisioning communicationProvisioning = null);

        void Stop();

        void Start();

        void PostMessage(IKey destination, IMessage message);
        void PostMessage(IEnumerable<IKey> destinations, IMessage message);

        void RegisterOnReceivedExtendedValidation(Func<ICommunicationChannel, IPEndPoint, int, bool> onReceiveExtendedValidation);
    }
}
