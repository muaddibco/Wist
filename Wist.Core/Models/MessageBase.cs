namespace Wist.Core.Models
{
    /// <summary>
    /// Base class for all message types
    /// Binary format of any message goes as follows:
    ///  * 2 bytes of message type
    ///  * 2 bytes of message body length
    ///  * N bytes of message body
    ///  * 64 bytes of signature
    ///  * 32 bytes of public key
    /// </summary>
    public class MessageBase
    {
        public MessageType MessageType { get; set; }

        public byte[] MessageBody { get; set; }

        public byte[] Signature { get; set; }

        public byte[] PublicKey { get; set; }
    }
}
