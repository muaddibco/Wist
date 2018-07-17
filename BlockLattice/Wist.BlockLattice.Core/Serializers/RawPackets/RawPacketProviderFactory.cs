using CommonServiceLocator;
using System.Collections.Generic;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.BlockLattice.Core.Serializers.RawPackets
{
    [RegisterDefaultImplementation(typeof(IRawPacketProvidersFactory), Lifetime = LifetimeManagement.Singleton)]
    public class RawPacketProvidersFactory : IRawPacketProvidersFactory
    {
        private readonly Stack<IRawPacketProvider> _rawPacketProviders;

        public RawPacketProvidersFactory()
        {
            _rawPacketProviders = new Stack<IRawPacketProvider>();
        }

        public IRawPacketProvider Create()
        {
            if(_rawPacketProviders.Count > 0)
            {
                return _rawPacketProviders.Pop();
            }
            else
            {
                IRawPacketProvider rawPacketProvider = ServiceLocator.Current.GetInstance<IRawPacketProvider>();

                return rawPacketProvider;
            }
        }

        public IRawPacketProvider Create(BlockBase blockBase)
        {
            IRawPacketProvider rawPacketProvider = Create();

            rawPacketProvider.Initialize(blockBase);

            return rawPacketProvider;
        }

        public void Utilize(IRawPacketProvider obj)
        {
            _rawPacketProviders.Push(obj);
        }
    }
}
