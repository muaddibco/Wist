using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Node.Core.Interfaces
{
    public interface INodeContext
    {
        byte[] PublicKey { get; }

        void Initialize();

        /// <summary>
        /// Signs message using Private Key of current Node
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        byte[] Sign(byte[] message);
    }
}
