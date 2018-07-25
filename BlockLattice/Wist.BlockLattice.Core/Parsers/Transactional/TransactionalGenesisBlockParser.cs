using System;
using System.IO;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Transactional;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Identity;
using Wist.Core.ProofOfWork;

namespace Wist.BlockLattice.Core.Parsers.Transactional
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionalGenesisBlockParser : TransactionalBlockParserBase
    {
        private readonly IIdentityKeyProvider _identityKeyProvider;
        private readonly IProofOfWorkCalculationRepository _proofOfWorkCalculationRepository;

        public TransactionalGenesisBlockParser(IProofOfWorkCalculationRepository proofOfWorkCalculationRepository, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) 
            : base(proofOfWorkCalculationRepository, identityKeyProvidersRegistry)
        {
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
            _proofOfWorkCalculationRepository = proofOfWorkCalculationRepository;
        }

        public override ushort BlockType => BlockTypes.Transaction_Genesis;

        public override PacketType PacketType => PacketType.TransactionalChain;

        protected override Span<byte> ParseTransactional(ushort version, Span<byte> spanBody, out TransactionalBlockBase transactionalBlockBase)
        {
            throw new NotImplementedException();
            //BlockBase block = null;

            //switch(version)
            //{
            //    case 1:
            //        block = new TransactionalGenesisBlock();
            //        TransactionalGenesisBlock genesisBlock = (TransactionalGenesisBlock)block;
            //        genesisBlock.ParkedFunds = br.ReadUInt64();
            //        byte[] delegatedAccountPK = br.ReadBytes(Globals.NODE_PUBLIC_KEY_SIZE);
            //        genesisBlock.DelegatedAccount = _identityKeyProvider.GetKey(delegatedAccountPK);
            //        byte verifiersCount = br.ReadByte();
            //        for (int i = 0; i < verifiersCount; i++)
            //        {
            //            genesisBlock.VerifierOriginalHashList.Add(br.ReadBytes(Globals.HASH_SIZE));
            //        }
            //        genesisBlock.RecoveryOriginalHash = br.ReadBytes(Globals.HASH_SIZE);
            //        genesisBlock.Nonce = br.ReadUInt64();
            //        genesisBlock.HashNonce = br.ReadBytes(Globals.HASH_SIZE);
            //        genesisBlock.HashPrev = br.ReadBytes(Globals.HASH_SIZE);
            //        break;
            //    default:
            //        throw new Exceptions.BlockVersionNotSupportedException(version, BlockTypes.Transaction_Genesis);
            //}
            
            //return block;
        }
    }
}
