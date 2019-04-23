using Chaos.NaCl;
using System;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Identity;
using Wist.Tests.Core.Fixtures;
using Xunit;
using NSubstitute;
using Wist.Core.HashCalculations;
using Wist.Core.Cryptography;
using Wist.Blockchain.Core.Parsers;
using CommonServiceLocator;
using Unity;
using Wist.Crypto.HashCalculations;
using Wist.Core.Models;
using Wist.Core.ExtensionMethods;
using Wist.Crypto.ConfidentialAssets;
using Wist.Crypto;

namespace Wist.Blockchain.Core.Tests
{
    public abstract class TestBase : IClassFixture<DependencyInjectionSupportFixture>
    {
        protected IIdentityKeyProvider _transactionHashKeyProvider;
        protected IIdentityKeyProvider _identityKeyProvider;
        protected IIdentityKeyProvidersRegistry _identityKeyProvidersRegistry;
        protected IHashCalculationsRepository _hashCalculationRepository;
        protected IBlockParsersRepositoriesRepository _blockParsersRepositoriesRepository;
        protected IBlockParsersRepository _blockParsersRepository;
        protected ISigningService _signingService;
        protected ISigningService _utxoSigningService;
        protected byte[] _privateKey;
        protected byte[] _privateViewKey;
        protected byte[] _publicKey;
        protected byte[] _expandedPrivateKey;

        public TestBase()
        {
            _transactionHashKeyProvider = Substitute.For<IIdentityKeyProvider>();
            _identityKeyProvider = Substitute.For<IIdentityKeyProvider>();
            _identityKeyProvidersRegistry = Substitute.For<IIdentityKeyProvidersRegistry>();
            _hashCalculationRepository = Substitute.For<IHashCalculationsRepository>();
            _blockParsersRepositoriesRepository = Substitute.For<IBlockParsersRepositoriesRepository>();
            _blockParsersRepository = Substitute.For<IBlockParsersRepository>();
            _signingService = Substitute.For<ISigningService>();

            _identityKeyProvidersRegistry.GetInstance().Returns(_identityKeyProvider);
            _identityKeyProvidersRegistry.GetTransactionsIdenityKeyProvider().Returns(_transactionHashKeyProvider);
            _identityKeyProvider.GetKey(null).ReturnsForAnyArgs(c => new Key32() { Value = c.Arg<Memory<byte>>() });
            _transactionHashKeyProvider.GetKey(null).ReturnsForAnyArgs(c => new Key16() { Value = c.Arg<Memory<byte>>() });
            _hashCalculationRepository.Create(HashType.MurMur).Returns(new MurMurHashCalculation());
            _blockParsersRepositoriesRepository.GetBlockParsersRepository(PacketType.Registry).ReturnsForAnyArgs(_blockParsersRepository);

            _privateKey = ConfidentialAssetsHelper.GetRandomSeed();
            _privateViewKey = ConfidentialAssetsHelper.GetRandomSeed();
            Ed25519.KeyPairFromSeed(out _publicKey, out _expandedPrivateKey, _privateKey);

            _signingService.WhenForAnyArgs(s => s.Sign(null, null)).Do(c => 
            {
                ((SignedPacketBase)c.ArgAt<IPacket>(0)).Signer = new Key32(_publicKey);
                ((SignedPacketBase)c.ArgAt<IPacket>(0)).Signature = Ed25519.Sign(((SignedPacketBase)c.ArgAt<IPacket>(0)).BodyBytes.ToArray(), _expandedPrivateKey);
            });
            _signingService.PublicKeys.Returns(new IKey[] { new Key32() { Value = _publicKey } });

            _utxoSigningService = new UtxoSigningService(_identityKeyProvidersRegistry);
            _utxoSigningService.Initialize(_privateKey, _privateViewKey);


            ServiceLocator.Current.GetInstance<IUnityContainer>().RegisterInstance(_blockParsersRepositoriesRepository);
        }
    }
}
