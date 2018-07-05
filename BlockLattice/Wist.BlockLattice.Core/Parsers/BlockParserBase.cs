using System.IO;
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

        public virtual BlockBase ParseBody(byte[] blockBody)
        {
            using (MemoryStream ms = new MemoryStream(blockBody))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    return ParseBody(br);
                }
            }
        }

        public virtual BlockBase Parse(byte[] source)
        {
            using (MemoryStream ms = new MemoryStream(source))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    SkipInitialBytes(br);

                    return ParseBody(br);
                }
            }
        }

        private BlockBase ParseBody(BinaryReader br)
        {
            ulong height = br.ReadUInt64();
            byte[] prevHash = br.ReadBytes(Globals.HASH_SIZE);
            ushort version = br.ReadUInt16();
            return Parse(version, height, prevHash, br);
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

        protected abstract BlockBase Parse(ushort version, ulong height, byte[] prevHash, BinaryReader br);
    }
}
