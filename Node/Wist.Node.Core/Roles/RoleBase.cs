using System;
using System.Collections.Generic;
using System.Text;
using Wist.Node.Core.Interfaces;

namespace Wist.Node.Core.Roles
{
    public abstract class RoleBase : IRole
    {
        protected bool _isInitialized;
        private readonly object _sync = new object();

        protected abstract void InitializeInner();

        public void Initialize()
        {
            if(_isInitialized)
            {
                return;
            }

            lock(_sync)
            {
                if(_isInitialized)
                {
                    return;
                }

                InitializeInner();
            }
        }

        public abstract void Start();
        public abstract void Stop();
    }
}
