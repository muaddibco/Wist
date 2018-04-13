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
    public interface IResponseFactory
    {
        /// <summary>
        /// Gets the op code.
        /// </summary>
        /// <value>The op code.</value>
        byte OpCode { get; }

        /// <summary>
        /// Creates the response.
        /// </summary>
        /// <param name="bodyBytes">The body bytes.</param>
        /// <returns>IResponse.</returns>
        IResponse CreateResponse(byte[] bodyBytes);
    }

    /// <summary>
    /// Interface IResponseFactory
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Savyon.Diagnostics.Communication.Common.HighLevel.IResponseFactory" />
    public interface IResponseFactory<out T> : IResponseFactory where T : IResponse
    {
        /// <summary>
        /// Creates the response.
        /// </summary>
        /// <param name="bodyBytes">The body bytes.</param>
        /// <returns>T.</returns>
        new T CreateResponse(byte[] bodyBytes);
    }
}
