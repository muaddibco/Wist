using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationLibrary.Sockets
{
    internal class AcceptOpUserToken
    {
        public AcceptOpUserToken(Int32 tokenId)
        {
            TokenId = tokenId;
        }

        public Int32 TokenId { get; }

        public Int32 SocketHandler { get; set; }
    }
}
