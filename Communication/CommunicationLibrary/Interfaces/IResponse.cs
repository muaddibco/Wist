using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;

namespace CommunicationLibrary.Interfaces
{
    /// <summary>
    /// Interface IResponse
    /// </summary>
    [ExtensionPoint]
    public interface IResponse
    {
        /// <summary>
        /// Gets the op code.
        /// </summary>
        /// <value>The op code.</value>
        byte OpCode { get; }

        byte[] ToByteArray();
    }
}
