using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.BlockLattice.Core
{
    [RegisterDefaultImplementation(typeof(IChainDataServicesManager), Lifetime = LifetimeManagement.Singleton)]
    public class ChainDataServicesManager : IChainDataServicesManager
    {
        private readonly IChainDataService[] _chainDataServices;
        public ChainDataServicesManager(IChainDataService[] chainDataServices)
        {
            _chainDataServices = chainDataServices;
        }

        public IChainDataService GetChainDataService(PacketType chainType)
        {
            return _chainDataServices.FirstOrDefault(c => c.ChainType == chainType);
        }
    }
}
