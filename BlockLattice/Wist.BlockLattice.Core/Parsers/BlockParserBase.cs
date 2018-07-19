using System.IO;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.ProofOfWork;

namespace Wist.BlockLattice.Core.Parsers
{
    public abstract class BlockParserBase : IBlockParser
    {
        public BlockParserBase()
        {
        }

        public abstract ushort BlockType { get; }

        public abstract PacketType PacketType { get; }

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
            ushort version = br.ReadUInt16();
            ushort messageType = br.ReadUInt16();
            BlockBase blockBase = Parse(version, br);

            return blockBase;
        }

        void SkipInitialBytes(BinaryReader br)
        {
            br.ReadBytes(2); // Packet Type

            ReadPowSection(br);
        }

        protected abstract void ReadPowSection(BinaryReader br);

        protected abstract BlockBase Parse(ushort version, BinaryReader br);
    }
}
