using CommunicationLibrary.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;

namespace CommunicationLibrary.Interfaces
{
    /// <summary>
    /// Interface IProtocolParser
    /// </summary>
    [ExtensionPoint]
    public interface IProtocolParser
    {
        /// <summary>
        /// Adds portion of bytes to the buffer of protocol parser for following fetching instances of <see cref="IMessage"/> from it
        /// </summary>
        /// <param name="buf"></param>
        void PushBuffer(byte[] buf);

        /// <summary>
        /// Iterates buffer collected so far and returns all instances of <see cref="IMessage"/> that were able to parse from it.
        /// </summary>
        /// <returns><see cref="IMessage"/></returns>
        IEnumerable<RawMessage> FetchAllMessages();
    }
}
