using log4net;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Wist.Core.Logging;
using Wist.Node.Core.Interfaces;

namespace Wist.Node.Core.Roles
{
    public abstract class ModuleBase : IModule
    {
        private readonly object _sync = new object();
        protected readonly ILogger _log;

        public ModuleBase(ILoggerService loggerService)
        {
            _log = loggerService.GetLogger(GetType().Name);
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
    }
}
