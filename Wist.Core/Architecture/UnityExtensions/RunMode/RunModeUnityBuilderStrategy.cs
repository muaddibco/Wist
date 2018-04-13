using System;
using System.Collections.Generic;
using System.Text;
using Unity.Builder;
using Unity.Builder.Strategy;

namespace Wist.Core.Architecture.UnityExtensions.RunMode
{
    class RunModeUnityBuilderStrategy : BuilderStrategy
    {
        public ExtendedUnityContainer Container { get; set; }

        public override void PreBuildUp(IBuilderContext context)
        {
            if (context.BuildKey.Name != null || Container.CurrentResolutionMode == Enums.RunMode.Default)
            {
                return;
            }

            string newName = Container.CurrentResolutionMode.ToString();

            if (Container.GetRegisteredNames(context.BuildKey.Type).Contains(newName))
            {
                context.BuildKey = new NamedTypeBuildKey(context.BuildKey.Type, newName);
            }
        }
    }
}
