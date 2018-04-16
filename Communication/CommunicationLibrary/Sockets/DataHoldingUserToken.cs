using CommunicationLibrary.Interfaces;
using System;
using System.Net.Sockets;

namespace CommunicationLibrary.Sockets
{
    internal class DataHoldingUserToken
    {
        public DataHoldingUserToken(IClientHandlerFactory clientHandlerFactory)
        {
            ClientHandler = clientHandlerFactory.Create();
        }

        public void Init(Int32 tokenId, Int32 offsetReceive, Int32 offsetSend)
        {
            TokenId = tokenId;
            BufferOffsetReceive = offsetReceive;
            BufferOffsetSend = offsetSend;
        }

        public Int32 TokenId { get; private set; }

        public Int32 BufferOffsetReceive { get; private set; }

        public Int32 BufferOffsetSend { get; private set; }

        public IClientHandler ClientHandler { get; }
    }
}
