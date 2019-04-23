using Unity;
using System.Collections.Generic;
using Wist.Blockchain.Core.DataModel;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Models;

namespace Wist.Blockchain.Core.Serializers.RawPackets
{
    [RegisterDefaultImplementation(typeof(IRawPacketProvidersFactory), Lifetime = LifetimeManagement.Singleton)]
    public class RawPacketProvidersFactory : IRawPacketProvidersFactory
    {
        private readonly Stack<IRawPacketProvider> _rawPacketProviders;
        private readonly IApplicationContext _applicationContext;

        public RawPacketProvidersFactory(IApplicationContext applicationContext)
        {
            _rawPacketProviders = new Stack<IRawPacketProvider>();
            _applicationContext = applicationContext;
        }

        public IRawPacketProvider Create()
        {
            if(_rawPacketProviders.Count > 0)
            {
                return _rawPacketProviders.Pop();
            }
            else
            {
                IRawPacketProvider rawPacketProvider = _applicationContext.Container.Resolve<RawPacketProvider>();

                return rawPacketProvider;
            }
        }

        public IRawPacketProvider Create(IPacket blockBase)
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
