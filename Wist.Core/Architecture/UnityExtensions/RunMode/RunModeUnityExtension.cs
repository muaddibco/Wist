using System;
using System.Collections.Generic;
using System.Text;
using Unity.Builder;
using Unity.Extension;

namespace Wist.Core.Architecture.UnityExtensions.RunMode
{
    class RunModeUnityExtension : UnityContainerExtension
    {
        private RunModeUnityBuilderStrategy _factoryUnityBuilderStrategy;

        protected override void Initialize()
        {
            _factoryUnityBuilderStrategy = new RunModeUnityBuilderStrategy
            {
                Container = Container as ExtendedUnityContainer
            };

            Context.Strategies.Add(_factoryUnityBuilderStrategy, UnityBuildStage.Setup);
        }
    }
}
