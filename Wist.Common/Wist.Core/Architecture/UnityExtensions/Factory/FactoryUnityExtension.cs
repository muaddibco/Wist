using System;
using System.Collections.Generic;
using System.Text;
using Unity.Builder;
using Unity.Extension;

namespace Wist.Core.Architecture.UnityExtensions.Factory
{
    public class FactoryUnityExtension : UnityContainerExtension
    {
        private FactoryUnityBuilderStrategy _factoryUnityBuilderStrategy;

        protected override void Initialize()
        {
            _factoryUnityBuilderStrategy = new FactoryUnityBuilderStrategy
            {
                Container = Container
            };

            Context.Strategies.Add(_factoryUnityBuilderStrategy, UnityBuildStage.PreCreation);
        }
    }
}
