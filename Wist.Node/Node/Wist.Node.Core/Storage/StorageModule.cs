using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Logging;
using Wist.Node.Core.Common;

namespace Wist.Node.Core.Storage
{
    [RegisterExtension(typeof(IModule), Lifetime = LifetimeManagement.Singleton)]
    public class StorageModule : ModuleBase
    {
        public const string NAME = nameof(StorageModule);

        public StorageModule(ILoggerService loggerService) : base(loggerService)
        {
        }

        public override string Name => NAME;

        public override void Start()
        {
            
        }

        protected override void InitializeInner()
        {
            
        }
    }
}
