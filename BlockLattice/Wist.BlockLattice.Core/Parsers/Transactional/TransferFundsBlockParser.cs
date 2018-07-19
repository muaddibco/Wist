using System.IO;
using Wist.BlockLattice.Core.DataModel.Transactional;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Exceptions;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Identity;
using Wist.Core.ProofOfWork;

namespace Wist.BlockLattice.Core.Parsers.Transactional
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class TransferFundsBlockParser : TransactionalBlockParserBase
    {
        public TransferFundsBlockParser(IProofOfWorkCalculationFactory proofOfWorkCalculationFactory, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) 
            : base(proofOfWorkCalculationFactory, identityKeyProvidersRegistry)
        {
        }

        public override ushort BlockType => BlockTypes.Transaction_TransferFunds;

        protected override TransactionalBlockBase ParseTransactional(ushort version, BinaryReader br)
        {
            TransactionalBlockBase block = null;

            if (version == 1)
            {
                byte[] target = br.ReadBytes(Globals.HASH_SIZE);
                ulong funds = br.ReadUInt64();

                block = new TransferFundsBlockV1()
                {
                    TargetOriginalHash = target,
                    UptodateFunds = funds,
                };
            }
            else
            {
                throw new BlockVersionNotSupportedException(version, BlockType);
            }

            return block;
        }
    }
}
