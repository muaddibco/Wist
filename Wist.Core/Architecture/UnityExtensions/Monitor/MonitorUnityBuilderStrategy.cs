using log4net;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Builder;
using Unity.Builder.Strategy;

namespace Wist.Core.Architecture.UnityExtensions.Monitor
{
    public class MonitorUnityBuilderStrategy : BuilderStrategy
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(MonitorUnityBuilderStrategy));

        public ExtendedUnityContainer Container { get; set; }

        public override void PreBuildUp(IBuilderContext context)
        {
            _log.Info($"Building {context.BuildKey.Type.FullName}");
        }
    }
}
