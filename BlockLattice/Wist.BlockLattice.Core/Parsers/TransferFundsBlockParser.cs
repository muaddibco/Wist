using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Transactional;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Exceptions;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.BlockLattice.Core.Parsers
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class TransferFundsBlockParser : BlockParserBase
    {
        public override BlockType BlockType => BlockType.Transaction_TransferFunds;

        public override void FillBlockBody(BlockBase block, byte[] blockBody)
        {
            throw new NotImplementedException();
        }

        protected override BlockBase Parse(BinaryReader br)
        {
            BlockBase block = null;
            ushort version = br.ReadUInt16();
            
            if (version == 1)
            {
                byte[] target = br.ReadBytes(64);
                ulong funds = br.ReadUInt64();
                byte[] nbackHash = br.ReadBytes(64);
                byte[] source = br.ReadBytes(64);

                block = new TransferFundsBlockV1()
                {
                    TargetOriginalHash = target,
                    UptodateFunds = funds,
                    NBackHash = nbackHash,
                    OriginalHash = source
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
