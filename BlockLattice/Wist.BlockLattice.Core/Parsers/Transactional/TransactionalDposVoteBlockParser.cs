using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Transactional;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Exceptions;
using Wist.Core.ProofOfWork;

namespace Wist.BlockLattice.Core.Parsers.Transactional
{
    public class TransactionalDposVoteBlockParser : TransactionalBlockParserBase
    {
        public TransactionalDposVoteBlockParser(IProofOfWorkCalculationFactory proofOfWorkCalculationFactory) : base(proofOfWorkCalculationFactory)
        {
        }

        public override ushort BlockType => BlockTypes.Transaction_Dpos;

        public override void FillBlockBody(BlockBase block, byte[] blockBody)
        {
            throw new NotImplementedException();
        }

        protected override TransactionalBlockBase ParseTransactional(ushort version, BinaryReader br)
        {
            TransactionalBlockBase block = null;

            if (version == 1)
            {
                byte[] nodeHash = br.ReadBytes(Globals.NODE_PUBLIC_KEY_SIZE);

                block = new TransactionalDposVote
                {
                    NodePublickKey = nodeHash
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
