using Wist.Core.Models;

namespace Wist.Blockchain.Core.DataModel
{
    public abstract class LinkedPacketBase : SignedPacketBase
    {
        /// <summary>
        /// 64 byte value of hash of previous block content
        /// </summary>
        public byte[] HashPrev { get; set; }
    }
}
