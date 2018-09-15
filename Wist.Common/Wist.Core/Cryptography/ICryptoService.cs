using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Identity;

namespace Wist.Core.Cryptography
{
    [ServiceContract]
    public interface ICryptoService
    {
        IKey Key { get; }
        /// <summary>
        /// Signs message using Private Key of current Node
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        byte[] Sign(byte[] message);

        bool Verify(byte[] signature, byte[] message, byte[] publickKey);

        void Initialize(byte[] privateKey);

        byte[] ComputeTransactionKey(byte[] bytes);
    }
}
