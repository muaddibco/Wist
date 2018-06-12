using log4net;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Wist.Node.Core.Interfaces;

namespace Wist.Node.Core.Roles
{
    public abstract class RoleBase : IRole
    {
        private readonly object _sync = new object();
        protected readonly ILog _log;

        public RoleBase()
        {
            _log = LogManager.GetLogger(GetType());
        }

        public abstract string Name { get; }

        public bool IsInitialized { get; private set; }

        public void Initialize()
        {
            if (IsInitialized)
                return;

            lock (_sync)
            {
                if (IsInitialized)
                    return;

                InitializeInner();

                IsInitialized = true;
            }
        }

        protected abstract void InitializeInner();

        public abstract Task Play();
    }
}
