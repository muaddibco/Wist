using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Transactional;
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
            BlockBase block = null;

            ushort version = br.ReadUInt16();

            switch(version)
            {
                case 1:
                    block = new TransactionalGenesisBlockV1();
                    TransactionalGenesisBlockV1 genesisBlock = (TransactionalGenesisBlockV1)block;
                    genesisBlock.OriginalHash = br.ReadBytes(Globals.HASH_SIZE);
                    byte verifiersCount = br.ReadByte();
                    for (int i = 0; i < verifiersCount; i++)
                    {
                        genesisBlock.VerifierOriginalHashList.Add(br.ReadBytes(Globals.HASH_SIZE));
                    }
                    genesisBlock.RecoveryOriginalHash = br.ReadBytes(Globals.HASH_SIZE);
                    genesisBlock.Nonce = br.ReadBytes(Globals.NONCE_SIZE);
                    genesisBlock.HashNonce = br.ReadBytes(Globals.HASH_SIZE);
                    genesisBlock.Hash = br.ReadBytes(Globals.HASH_SIZE);
                    break;
                default:
                    throw new Exceptions.BlockVersionNotSupportedException(version, BlockTypes.Transaction_Genesis);
            }
            
            return block;
        }
    }
}
