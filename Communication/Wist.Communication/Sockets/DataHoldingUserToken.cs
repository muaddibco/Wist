using Wist.Communication.Interfaces;
using System;
using System.Net.Sockets;

namespace Wist.Communication.Sockets
{
    internal class DataHoldingUserToken
    {
        private SocketAsyncEventArgs _socketAsyncEventArgs;

        public DataHoldingUserToken(IClientHandlerFactory clientHandlerFactory)
        {
            ClientHandler = clientHandlerFactory.Create();
        }

        public void Init(SocketAsyncEventArgs socketAsyncEventArgs, Int32 tokenId, Int32 offsetReceive, Int32 offsetSend, Int32 receiveSendMaxBufferSize)
        {
            _socketAsyncEventArgs = socketAsyncEventArgs;
            TokenId = tokenId;
            BufferOffsetReceive = offsetReceive;
            BufferOffsetSend = offsetSend;
            ReceiveSendMaxBufferSize = receiveSendMaxBufferSize;
        }

        public Int32 TokenId { get; private set; }

        public Int32 BufferOffsetReceive { get; private set; }

        public Int32 BufferOffsetSend { get; private set; }

        public Int32 ReceiveSendMaxBufferSize { get; private set; }

        public IClientHandler ClientHandler { get; }
    }
}
