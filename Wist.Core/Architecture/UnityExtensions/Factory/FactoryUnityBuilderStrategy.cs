using log4net;
using System;
using System.Collections.Generic;
using System.Text;
using Unity;
using Unity.Builder;
using Unity.Builder.Strategy;
using Wist.Core.Exceptions;
using Wist.Core.ExtensionMethods;

namespace Wist.Core.Architecture.UnityExtensions.Factory
{
    class FactoryUnityBuilderStrategy : BuilderStrategy
    {
        public IUnityContainer Container { get; set; }
        private readonly Dictionary<Type, ITypeFactory> _factoryCache = new Dictionary<Type, ITypeFactory>();
        private ILog _log = LogManager.GetLogger(typeof(FactoryUnityBuilderStrategy));

        public override void PreBuildUp(IBuilderContext context)
        {
            string typeName = context.BuildKey.Type.Name;
            _log.Debug($"Processing type [{context.BuildKey.Name}]: {typeName}");
            base.PreBuildUp(context);

            if (context.Existing != null) return;

            ITypeFactory factory;

            if (_factoryCache.ContainsKey(context.BuildKey.Type) == false)
            {
                var registerTypeAttribute = context.BuildKey.Type.GetAttribute<RegisterType>();
                if (registerTypeAttribute?.Factory == null)
                {
                    factory = null;
                }
                else
                {
                    factory = Container.Resolve(registerTypeAttribute.Factory) as ITypeFactory;

                    if (factory == null)
                        throw new FactoryTypeResolutionFailureException(registerTypeAttribute.Factory);
                }

                _factoryCache.Add(context.BuildKey.Type, factory);
            }
            else
            {
                factory = _factoryCache[context.BuildKey.Type];
            }

            if (factory == null) return;

            context.Existing = factory.CreateInstance(context.BuildKey.Type);
        }
    }
}
