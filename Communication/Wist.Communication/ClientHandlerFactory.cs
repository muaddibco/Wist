using CommonServiceLocator;
using Wist.Communication.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Communication
{
    public class ClientHandlerFactory : IClientHandlerFactory
    {
        private readonly Stack<IClientHandler> _handlers;

        public ClientHandlerFactory()
        {
            _handlers = new Stack<IClientHandler>();
        }

        public IClientHandler Create()
        {
            if(_handlers.Count > 1)
            {
                return _handlers.Pop();
            }

            return ServiceLocator.Current.GetInstance<IClientHandler>();
        }

        public void Utilize(IClientHandler handler)
        {
            _handlers.Push(handler);
        }
    }
}
