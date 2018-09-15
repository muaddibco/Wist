using CommonServiceLocator;
using System;
using System.Collections.Generic;
using Unity;
using Wist.Network.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Network.Communication
{
    [RegisterDefaultImplementation(typeof(IBufferManagerFactory), Lifetime = LifetimeManagement.Singleton)]
    public class BufferManagerFactory : IBufferManagerFactory
    {
        private readonly Stack<IBufferManager> _bufferManagersPool = new Stack<IBufferManager>();
        private readonly IApplicationContext _applicationContext;

        public BufferManagerFactory(IApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public IBufferManager Create()
        {
            if (_bufferManagersPool.Count > 0)
            {
                lock (_bufferManagersPool)
                {
                    if (_bufferManagersPool.Count > 0)
                    {
                        return _bufferManagersPool.Pop();
                    }
                    else
                    {
                        
                        return _applicationContext.Container.Resolve<IBufferManager>();
                    }
                }
            }
            else
            {
                return _applicationContext.Container.Resolve<IBufferManager>();
            }
        }

        public void Utilize(IBufferManager bufferManager)
        {
            if (bufferManager == null)
            {
                throw new ArgumentNullException(nameof(bufferManager));
            }

            _bufferManagersPool.Push(bufferManager);
        }
    }

}
