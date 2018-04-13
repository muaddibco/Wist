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
        /// Parses the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns>IResponse.</returns>
        IResponse Parse(byte[] input, out int start, out int end);
    }
}
