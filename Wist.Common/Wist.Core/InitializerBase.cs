using Wist.Core.Architecture.Enums;

namespace Wist.Core
{
    public abstract class InitializerBase : IInitializer
    {
        private readonly object _sync = new object();

        public bool Initialized { get; private set; }

        public abstract ExtensionOrderPriorities Priority { get; }

        public void Initialize()
        {
            if(Initialized)
            {
                return;
            }

            lock(_sync)
            {
                if(Initialized)
                {
                    return;
                }

                InitializeInner();

                Initialized = true;
            }
        }

        protected abstract void InitializeInner();
    }
}
