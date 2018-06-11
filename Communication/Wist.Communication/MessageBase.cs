using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wist.Communication.Exceptions;
using Wist.Communication.Interfaces;
using Wist.Core.Communication;

namespace Wist.Communication
{
    public abstract class MessageBase : IMessage
    {
        public const byte DLE = 0x10;
        public const byte STX = 0x02;

        public byte[] GetBytes()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(DLE);
                    bw.Write(STX);

                    byte[] bodyBytes = GetBodyBytes();

                    if(bodyBytes.Length > ushort.MaxValue)
                    {
                        throw new MaxMessageSizeExceededException();
                    }

                    ushort packetLength = (ushort)bodyBytes.Length;

                    byte[] lengthByte = BitConverter.GetBytes(packetLength);
                    WriteByteWithEncoding(bw, lengthByte[0]);
                    WriteByteWithEncoding(bw, lengthByte[1]);

                    foreach (byte b in bodyBytes)
                    {
                        WriteByteWithEncoding(bw, b);
                    }
                }

                return ms.ToArray();
            }
        }

        private void WriteByteWithEncoding(BinaryWriter bw, byte b)
        {
            if(b == DLE || b == STX)
            {
                bw.Write(DLE);
                b += DLE;
            }

            bw.Write(b);
        }

        protected abstract byte[] GetBodyBytes();
    }
}
