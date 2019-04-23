using log4net;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Builder;
using Unity.Builder.Strategy;
using Unity.Registration;

namespace Wist.Core.Architecture.UnityExtensions.Monitor
{
    /// <summary>
    /// This class is used for logging components being constructed by container
    /// </summary>
    public class MonitorUnityBuilderStrategy : BuilderStrategy
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(MonitorUnityBuilderStrategy));

        public ExtendedUnityContainer Container { get; set; }

        /// <summary>
        /// This class is used for logging components being constructed by container
        /// </summary>
        public MonitorUnityBuilderStrategy()
        {

        }

        public override void PreBuildUp(IBuilderContext context)
        {
            ContainerRegistration containerRegistration = context.Registration as ContainerRegistration;
            InternalRegistration internalRegistration = context.Registration as InternalRegistration;

            if (containerRegistration != null) 
            {
                _log.Info($"Building {containerRegistration.RegisteredType.FullName} mapped to {containerRegistration.MappedToType.FullName}");
            }
            else if (internalRegistration != null)
            {
                _log.Info($"Building {internalRegistration.Type.FullName}");
            }
            else
            {
                _log.Info($"Building {context.BuildKey.Type.FullName}");
            }
        }
    }
}
