using CommonServiceLocator;
using System;
using System.Collections.Generic;
using System.Text;
using Wist.Communication.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Communication.Sockets
{
    [RegisterDefaultImplementation(typeof(IBufferManagerFactory), Lifetime = LifetimeManagement.Singleton)]
    public class BufferManagerFactory : IBufferManagerFactory
    {
        private readonly Stack<IBufferManager> _bufferManagersPool = new Stack<IBufferManager>();

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
                        return ServiceLocator.Current.GetInstance<IBufferManager>();
                    }
                }
            }
            else
            {
                return ServiceLocator.Current.GetInstance<IBufferManager>();
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
