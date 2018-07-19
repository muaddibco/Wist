using System;
using System.Collections.Generic;
using System.Text;
using Unity.Builder;
using Unity.Extension;

namespace Wist.Core.Architecture.UnityExtensions.RunMode
{
    class RunModeUnityExtension : UnityContainerExtension
    {
        private RunModeUnityBuilderStrategy _runModeUnityBuilderStrategy;

        protected override void Initialize()
        {
            _runModeUnityBuilderStrategy = new RunModeUnityBuilderStrategy
            {
                Container = Container as ExtendedUnityContainer
            };

            Context.Strategies.Add(_runModeUnityBuilderStrategy, UnityBuildStage.Setup);
        }
    }
}
