﻿using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;

namespace CommunicationLibrary.Interfaces
{
    /// <summary>
    /// Service receives raw arrays of bytes representing all types of messages exchanges over network. 
    /// Byte arrays must contain exact bytes of message to be processed correctly.
    /// </summary>
    [ExtensionPoint]
    public interface IPacketsHandler
    {
        /// <summary>
        /// Bytes being pushed to <see cref="IPacketsHandler"/> must form complete packet for following validation and processing
        /// </summary>
        /// <param name="messagePacket">Bytes of complete message for following processing</param>
        void Push(byte[] messagePacket);

        void Start(bool withErrorsProcessing = true);

        void Stop();
    }
}