using System;
using System.Buffers.Binary;
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
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
    public class TransferFundsBlockParser : TransactionalBlockParserBase
    {
        public TransferFundsBlockParser(IProofOfWorkCalculationRepository proofOfWorkCalculationRepository, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) 
            : base(proofOfWorkCalculationRepository, identityKeyProvidersRegistry)
        {
        }

        public override ushort BlockType => BlockTypes.Transaction_TransferFunds;

        protected override Span<byte> ParseTransactional(ushort version, Span<byte> spanBody, out TransactionalBlockBase transactionalBlockBase)
        {
            TransactionalBlockBase block = null;

            if (version == 1)
            {
                byte[] target = spanBody.Slice(0, Globals.HASH_SIZE).ToArray();
                ulong funds = BinaryPrimitives.ReadUInt64LittleEndian(spanBody.Slice(Globals.HASH_SIZE));

                block = new TransferFundsBlockV1()
                {
                    TargetOriginalHash = target,
                    UptodateFunds = funds,
                };

                transactionalBlockBase = block;

                return spanBody.Slice(Globals.HASH_SIZE + 8);
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}
