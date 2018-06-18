using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Wist.Core.Communication
{
    public interface IPacketProvider
    {
        void WriteBuffer(BinaryWriter bw, BinaryReader br);
    }
}
