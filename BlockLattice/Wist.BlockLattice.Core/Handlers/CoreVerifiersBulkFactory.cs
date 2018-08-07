using Unity;
using System.Collections.Generic;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.BlockLattice.Core.Handlers
{
    [RegisterDefaultImplementation(typeof(ICoreVerifiersBulkFactory), Lifetime = LifetimeManagement.Singleton)]
    public class CoreVerifiersBulkFactory : ICoreVerifiersBulkFactory
    {
        private readonly ICoreVerifier[] _coreVerifiers;
        private readonly IApplicationContext _applicationContext;

        public CoreVerifiersBulkFactory(ICoreVerifier[] coreVerifiers, IApplicationContext applicationContext)
        {
            _coreVerifiers = coreVerifiers;
            _applicationContext = applicationContext;
        }

        public IEnumerable<ICoreVerifier> Create()
        {
            List<ICoreVerifier> coreVerifiers = new List<ICoreVerifier>();

            if (_coreVerifiers != null)
            {
                foreach (ICoreVerifier coreVerifier in _coreVerifiers)
                {
                    coreVerifiers.Add((ICoreVerifier)_applicationContext.Container.Resolve(coreVerifier.GetType()));
                }
            }

            return coreVerifiers;
        }
    }
}
