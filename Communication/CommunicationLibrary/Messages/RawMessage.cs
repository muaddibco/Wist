using CommunicationLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationLibrary.Messages
{
    public class RawMessage
    {
        public RawMessage(byte code, byte[] body, Int16 crc)
        {
            MessageCode = code;
            Body = body;
            Crc = crc;
        }

        public byte MessageCode { get; }

        public byte[] Body { get; }

        public Int16 Crc { get; }
    }
}
