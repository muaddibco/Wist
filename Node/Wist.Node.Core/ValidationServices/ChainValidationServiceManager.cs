using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;
using Wist.Node.Core.Interfaces;

namespace Wist.Node.Core.ValidationServices
{
    [RegisterDefaultImplementation(typeof(IChainValidationServiceManager), Lifetime = Wist.Core.Architecture.Enums.LifetimeManagement.Singleton)]
    public class ChainValidationServiceManager : IChainValidationServiceManager
    {
        private readonly IChainValidationService[] _chainValidationServices;

        public ChainValidationServiceManager(IChainValidationService[] chainValidationServices)
        {
            _chainValidationServices = chainValidationServices;
        }

        public IChainValidationService GetChainValidationService(PacketType chainType)
        {
            //TODO: add check and dedicated exception 
            return _chainValidationServices.FirstOrDefault(c => c.ChainType == chainType);
        }
    }
}
