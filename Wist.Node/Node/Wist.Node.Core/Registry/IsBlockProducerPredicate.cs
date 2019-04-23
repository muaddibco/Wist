using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Predicates;

namespace Wist.Node.Core.Registry
{
    [RegisterExtension(typeof(IPredicate), Lifetime = LifetimeManagement.Singleton)]
    public class IsBlockProducerPredicate : IPredicate
    {
        public string Name => "IsBlockProducer";

        public bool Evaluate(params object[] args)
        {
            //TODO: replace with getting from configuration
            return true;
        }
    }
}
