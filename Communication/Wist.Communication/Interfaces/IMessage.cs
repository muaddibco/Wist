using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;

namespace Wist.Communication.Interfaces
{
    /// <summary>
    /// Interface IResponse
    /// </summary>
    [ExtensionPoint]
    public interface IMessage
    {
        /// <summary>
        /// Gets the op code.
        /// </summary>
        /// <value>The op code.</value>
        byte MessageCode { get; }

        byte[] ToByteArray();
    }
}
