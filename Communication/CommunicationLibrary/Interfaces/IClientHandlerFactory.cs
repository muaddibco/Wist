using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationLibrary.Interfaces
{
    public interface IClientHandlerFactory
    {
        IClientHandler Create();

        void Utilize(IClientHandler handler);
    }
}
