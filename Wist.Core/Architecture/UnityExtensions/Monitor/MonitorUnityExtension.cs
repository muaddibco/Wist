using System;
using System.Collections.Generic;
using System.Text;
using Unity.Builder;
using Unity.Extension;

namespace Wist.Core.Architecture.UnityExtensions.Monitor
{
    /// <summary>
    /// This class is used for logging components being constructed by container
    /// </summary>
    public class MonitorUnityExtension : UnityContainerExtension
    {
        private MonitorUnityBuilderStrategy _monitorUnityBuilderStrategy;

        /// <summary>
        /// This class is used for logging components being constructed by container
        /// </summary>
        public MonitorUnityExtension()
        {

        }

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
