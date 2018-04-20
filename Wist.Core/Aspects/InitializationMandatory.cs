using PostSharp.Aspects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Wist.Core.Exceptions;

namespace Wist.Core.Aspects
{
    [Serializable]
    public class InitializationMandatoryAttribute : OnMethodBoundaryAspect
    {
        [DebuggerStepThrough]
        public override bool CompileTimeValidate(MethodBase method)
        {
            if(!typeof(ISupportInitialization).IsAssignableFrom(method.DeclaringType))
            {
                throw new MandatoryInterfaceNotImplementedException(GetType(), typeof(ISupportInitialization), method.DeclaringType);
            }

            return true;
        }

        [DebuggerStepThrough]
        public override void OnEntry(MethodExecutionArgs args)
        {
            ISupportInitialization supportInitialization = args.Instance as ISupportInitialization;

            if (supportInitialization == null)
                return;

            if(!supportInitialization.IsInitialized)
            {
                throw new InstanceIsNotInitializedException(args.Instance.GetType());
            }
        }
    }
}
