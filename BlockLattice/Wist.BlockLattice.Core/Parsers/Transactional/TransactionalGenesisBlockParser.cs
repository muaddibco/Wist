using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.BlockLattice.Core.Parsers.Transactional
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class TransactionalGenesisBlockParser : TransactionalBlockParserBase
    {
        public override ushort BlockType => BlockTypes.Transaction_Genesis;

        public override void FillBlockBody(BlockBase block, byte[] blockBody)
        {
            throw new NotImplementedException();
        }

        protected override BlockBase Parse(BinaryReader br)
        {
            throw new NotImplementedException();
        }
    }
}
