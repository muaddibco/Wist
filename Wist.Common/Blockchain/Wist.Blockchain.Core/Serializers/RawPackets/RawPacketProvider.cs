using Wist.Blockchain.Core.Exceptions;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.HashCalculations;
using Wist.Core.Identity;
using Wist.Core.Models;

namespace Wist.Blockchain.Core.Serializers.RawPackets
{

    [RegisterExtension(typeof(IRawPacketProvider), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class RawPacketProvider : IRawPacketProvider
    {
        private bool _disposed = false; // To detect redundant calls
        private IPacket _blockBase;
        private readonly IRawPacketProvidersFactory _rawPacketProvidersFactory;
        private readonly IHashCalculation _transactionKeyHashCalculation;
        private readonly IIdentityKeyProvider _transactionKeyIdentityKeyProvider;

        public RawPacketProvider(IRawPacketProvidersFactory rawPacketProvidersFactory, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, IHashCalculationsRepository hashCalculationsRepository)
        {
            _rawPacketProvidersFactory = rawPacketProvidersFactory;
            _transactionKeyIdentityKeyProvider = identityKeyProvidersRegistry.GetTransactionsIdenityKeyProvider();
            _transactionKeyHashCalculation = hashCalculationsRepository.Create(HashType.MurMur);
        }

        public byte[] GetBytes()
        {
            if(_blockBase?.RawData == null)
            {
                throw new RawBytesNotInitializedException();
            }

            return _blockBase.RawData.ToArray();
        }

        public void Initialize(IPacket blockBase)
        {
            _disposed = false;
            _blockBase = blockBase;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _rawPacketProvidersFactory.Utilize(this);

                _disposed = true;
            }
        }

        //TODO: this function is too resource intensive - need to find another way of transaction key obtaining
        public IKey GetKey()
        {
            byte[] hash = _transactionKeyHashCalculation.CalculateHash(_blockBase.RawData);
            IKey key = _transactionKeyIdentityKeyProvider.GetKey(hash);

            return key;
        }
    }
}
