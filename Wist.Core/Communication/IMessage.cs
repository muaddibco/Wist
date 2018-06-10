using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Core.Communication
{
    public interface IMessage
    {
        byte[] GetBytes();
    }
}
