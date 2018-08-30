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
using Wist.Core.HashCalculations;

namespace Wist.BlockLattice.Core.Parsers.Transactional
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
    public class AcceptFundsBlockParser : TransactionalBlockParserBase
    {
        public AcceptFundsBlockParser(IHashCalculationsRepository proofOfWorkCalculationRepository, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) : base(proofOfWorkCalculationRepository, identityKeyProvidersRegistry)
        {
        }

        public override ushort BlockType => BlockTypes.Transaction_AcceptFunds;

        protected override Span<byte> ParseTransactional(ushort version, Span<byte> spanBody, out TransactionalBlockBase transactionalBlockBase)
        {
            TransactionalBlockBase block = null;

            if (version == 1)
            {
                byte[] origin = spanBody.Slice(0, Globals.DEFAULT_HASH_SIZE).ToArray();
                ulong funds = BinaryPrimitives.ReadUInt64LittleEndian(spanBody.Slice(Globals.DEFAULT_HASH_SIZE));

                block = new AcceptFundsBlockV1()
                {
                    SourceOriginalHash = origin,
                    UptodateFunds = funds,
                };

                transactionalBlockBase = block;

                return spanBody.Slice(Globals.DEFAULT_HASH_SIZE + 8);
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}
