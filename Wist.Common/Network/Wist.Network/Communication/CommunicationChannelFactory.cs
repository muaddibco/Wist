using Unity;
using Wist.Network.Interfaces;
using System.Collections.Generic;
using Wist.Core.Architecture;

namespace Wist.Network.Communication
{
    public class CommunicationChannelFactory : ICommunicationChannelFactory
    {
        private readonly Stack<ICommunicationChannel> _handlers;
        private readonly IApplicationContext _applicationContext;

        public CommunicationChannelFactory(IApplicationContext applicationContext)
        {
            _handlers = new Stack<ICommunicationChannel>();
            _applicationContext = applicationContext;
        }

        public ICommunicationChannel Create()
        {
            if(_handlers.Count > 1)
            {
                return _handlers.Pop();
            }

            return _applicationContext.Container.Resolve<ICommunicationChannel>();
        }

        public void Utilize(ICommunicationChannel handler)
        {
            _handlers.Push(handler);
        }
    }
}
