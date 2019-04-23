using System.Threading;
using Wist.Core.Logging;

namespace Wist.Core.Modularity
{
    public abstract class ModuleBase : IModule
    {
        private readonly object _sync = new object();
        protected readonly ILogger _log;
        protected CancellationToken _cancellationToken;

        public ModuleBase(ILoggerService loggerService)
        {
            _log = loggerService.GetLogger(GetType().Name);
        }

        public abstract string Name { get; }

        public bool IsInitialized { get; private set; }

        public void Initialize(CancellationToken ct)
        {
            if (IsInitialized)
                return;

            _cancellationToken = ct;

            lock (_sync)
            {
                if (IsInitialized)
                    return;

                InitializeInner();

                IsInitialized = true;
            }
        }

        protected abstract void InitializeInner();
        public abstract void Start();
    }
}
