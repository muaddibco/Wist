using CommunicationLibrary.Interfaces;
using System;
using System.Net.Sockets;

namespace CommunicationLibrary.Sockets
{
    internal class DataHoldingUserToken
    {
        public DataHoldingUserToken(Int32 tokenId, Int32 offsetReceive, Int32 offsetSend, IProtocolParser protocolParser)
        {
            TokenId = tokenId;
            BufferOffsetReceive = offsetReceive;
            BufferOffsetSend = offsetSend;
            ProtocolParser = protocolParser;
        }

        public Int32 TokenId { get; }

        public Int32 BufferOffsetReceive { get; }

        public Int32 BufferOffsetSend { get; }

        public IProtocolParser ProtocolParser { get; set; }
    }
}
