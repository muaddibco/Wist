using System;
using System.Collections.Generic;
using System.Text;
using Unity.Builder;
using Unity.Extension;

namespace Wist.Core.Architecture.UnityExtensions.Monitor
{
    public class MonitorUnityExtension : UnityContainerExtension
    {
        private MonitorUnityBuilderStrategy _monitorUnityBuilderStrategy;

        protected override void Initialize()
        {
            _monitorUnityBuilderStrategy = new MonitorUnityBuilderStrategy()
            {
                Container = Container as ExtendedUnityContainer
            };

            Context.Strategies.Add(_monitorUnityBuilderStrategy, UnityBuildStage.PreCreation);
        }
    }
}
