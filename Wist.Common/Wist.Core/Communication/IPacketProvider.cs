using System;

namespace Wist.Core.Communication
{
    public interface IPacketProvider : IDisposable
    {
        byte[] GetBytes();
    }
}
