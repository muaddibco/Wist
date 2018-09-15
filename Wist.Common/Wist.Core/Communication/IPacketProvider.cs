using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Wist.Core.Communication
{
    public interface IPacketProvider : IDisposable
    {
        byte[] GetBytes();
    }
}
