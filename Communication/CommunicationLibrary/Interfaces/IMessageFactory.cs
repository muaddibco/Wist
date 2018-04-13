using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;

namespace CommunicationLibrary.Interfaces
{
    /// <summary>
    /// Interface IResponseFactory
    /// </summary>
    [ExtensionPoint]
    public interface IMessageFactory
    {
        /// <summary>
        /// Gets the op code.
        /// </summary>
        /// <value>The op code.</value>
        byte MessageCode { get; }

        /// <summary>
        /// Creates the response.
        /// </summary>
        /// <param name="bodyBytes">The body bytes.</param>
        /// <returns>IResponse.</returns>
        IMessage CreateResponse(byte[] bodyBytes);
    }

    /// <summary>
    /// Interface IResponseFactory
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Savyon.Diagnostics.Communication.Common.HighLevel.IResponseFactory" />
    public interface IMessageFactory<out T> : IMessageFactory where T : IMessage
    {
        /// <summary>
        /// Creates the response.
        /// </summary>
        /// <param name="bodyBytes">The body bytes.</param>
        /// <returns>T.</returns>
        new T CreateResponse(byte[] bodyBytes);
    }
}
