using System;
using System.Net.Sockets;

namespace CommunicationLibrary.Sockets
{
    internal class DataHoldingUserToken
    {
        public DataHoldingUserToken(Int32 tokenId, Int32 offsetReceive, Int32 offsetSend)
        {
            TokenId = tokenId;
            BufferOffsetReceive = offsetReceive;
            BufferOffsetSend = offsetSend;
        }

        public Int32 TokenId { get; }

        public Int32 BufferOffsetReceive { get; }

        public Int32 BufferOffsetSend { get; }
    }
}
