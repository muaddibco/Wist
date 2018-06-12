using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.ProofOfWork;

namespace Wist.BlockLattice.Core.Parsers
{
    public abstract class BlockParserBase : IBlockParser
    {
        private readonly IProofOfWorkCalculationFactory _proofOfWorkCalculationFactory;

        public BlockParserBase(IProofOfWorkCalculationFactory proofOfWorkCalculationFactory)
        {
            _proofOfWorkCalculationFactory = proofOfWorkCalculationFactory;
        }

        public abstract ushort BlockType { get; }

        public abstract PacketType ChainType { get; }

        public abstract void FillBlockBody(BlockBase block, byte[] blockBody);

        public virtual BlockBase Parse(byte[] source)
        {
            BlockBase block;
            using (MemoryStream ms = new MemoryStream(source))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    SkipInitialBytes(br);
                    ushort version = br.ReadUInt16();
                    block = Parse(version, br);
                }
            }

            return block;
        }

        void SkipInitialBytes(BinaryReader br)
        {
            br.ReadBytes(2);
            POWType powType = (POWType)br.ReadUInt16();
            br.ReadUInt32();
            br.ReadUInt64();
            br.ReadBytes(_proofOfWorkCalculationFactory.Create(powType).HashSize);
            ushort blockType = br.ReadUInt16();
        }

        protected abstract BlockBase Parse(ushort version, BinaryReader br);
    }
}
