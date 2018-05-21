using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Transactional;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Exceptions;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.ProofOfWork;

namespace Wist.BlockLattice.Core.Parsers.Transactional
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class AcceptFundsBlockParser : TransactionalBlockParserBase
    {
        public AcceptFundsBlockParser(IProofOfWorkCalculationFactory proofOfWorkCalculationFactory) : base(proofOfWorkCalculationFactory)
        {
        }

        public override ushort BlockType => BlockTypes.Transaction_AcceptFunds;

        public override void FillBlockBody(BlockBase block, byte[] blockBody)
        {
            throw new NotImplementedException();
        }

        protected override TransactionalBlockBase ParseTransactional(ushort version, BinaryReader br)
        {
            TransactionalBlockBase block = null;

            if (version == 1)
            {
                byte[] origin = br.ReadBytes(64);
                ulong funds = br.ReadUInt64();

                block = new AcceptFundsBlockV1()
                {
                    SourceOriginalHash = origin,
                    UptodateFunds = funds
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
