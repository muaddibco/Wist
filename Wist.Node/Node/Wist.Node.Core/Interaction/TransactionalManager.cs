using System;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Wist.BlockLattice.Core;
using Wist.BlockLattice.Core.DataModel.Transactional;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.HashCalculations;
using Wist.Core.Identity;
using Wist.Proto.Model;

namespace Wist.Node.Core.Interaction
{
    public class TransactionalChainManagerImpl : TransactionalChainManager.TransactionalChainManagerBase
    {
        private readonly IChainDataService _chainDataService;
        private readonly IIdentityKeyProvider _identityKeyProvider;
        private readonly IHashCalculation _hashCalculation;

        public TransactionalChainManagerImpl(IChainDataServicesManager chainDataServicesManager, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, IHashCalculationsRepository hashCalculationsRepository)
        {
            _chainDataService = chainDataServicesManager.GetChainDataService(PacketType.TransactionalChain);
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
            _hashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
        }

        public override Task<TransactionalBlockEssense> GetLastTransactionalBlock(TransactionalBlockRequest request, ServerCallContext context)
        {
            if (request.PublicKey == null)
            {
                throw new ArgumentNullException(nameof(request.PublicKey));
            }

            byte[] keyBytes = request.PublicKey.ToByteArray();

            if (keyBytes.Length != Globals.NODE_PUBLIC_KEY_SIZE)
            {
                throw new ArgumentException($"Public key size must be of {Globals.NODE_PUBLIC_KEY_SIZE} bytes");
            }

            IKey key = _identityKeyProvider.GetKey(keyBytes);
            TransactionalBlockBase transactionalBlockBase = (TransactionalBlockBase)_chainDataService.GetLastBlock(key);

            TransactionalBlockEssense transactionalBlockEssense = new TransactionalBlockEssense
            {
                Height = transactionalBlockBase?.BlockHeight ?? 0,
                //TODO: need to reconsider hash calculation here since it is potential point of DoS attack
                Hash = ByteString.CopyFrom(transactionalBlockBase != null ? _hashCalculation.CalculateHash(transactionalBlockBase.NonHeaderBytes) : new byte[Globals.DEFAULT_HASH_SIZE]),
                UpToDateFunds = transactionalBlockBase?.UptodateFunds ?? 0
            };

            return Task.FromResult(transactionalBlockEssense);
        }
    }
}
