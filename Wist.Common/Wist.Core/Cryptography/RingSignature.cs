using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Core.Cryptography
{
    public class RingSignature
    {
        /// <summary>
        /// 32 byte array
        /// </summary>
        public byte[] C { get; set; }

        /// <summary>
        /// 32 byte array
        /// </summary>
        public byte[] R { get; set; }
    }
}
