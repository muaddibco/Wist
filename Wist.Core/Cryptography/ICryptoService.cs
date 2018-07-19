using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;

namespace Wist.Core.Cryptography
{
    [ServiceContract]
    public interface ICryptoService
    {
        /// <summary>
        /// Signs message using Private Key of current Node
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        byte[] Sign(byte[] message);

        bool Verify(byte[] signature, byte[] message, byte[] publickKey);



        void Initialize(byte[] privateKey);
    }
}
