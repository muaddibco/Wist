using System;
using System.Buffers.Binary;
using System.IO;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.ExtensionMethods;
using Wist.Core.HashCalculations;

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
            Span<byte> span = new Span<byte>(blockBody);

            BlockBase blockBase = ParseBody(span);

            return blockBase;
        }

        public virtual BlockBase Parse(byte[] source)
        {
            Span<byte> span = new Span<byte>(source);
            
            Span<byte> spanHeader;
            Span<byte> spanBody = SliceInitialBytes(span, out spanHeader);

            BlockBase blockBase = ParseBody(spanBody);

            FillBlockBaseHeader(blockBase, spanHeader);

            blockBase.RawData = source;

            return blockBase;
        }

        private BlockBase ParseBody(Span<byte> spanBody)
        {
            ushort version = BinaryPrimitives.ReadUInt16LittleEndian(spanBody);
            ushort messageType = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Slice(2));
            BlockBase blockBase = ParseBlockBase(version, spanBody.Slice(4));

            blockBase.BodyBytes = GetBodyBytes(spanBody);

            return blockBase;
        }

        protected virtual byte[] GetBodyBytes(Span<byte> spanBody)
        {
            return spanBody.ToArray();
        }

        protected virtual Span<byte> SliceInitialBytes(Span<byte> span, out Span<byte> spanHeader)
        {
            spanHeader = span.Slice(0, 2);

            return span.Slice(2);
        }

        protected abstract BlockBase ParseBlockBase(ushort version, Span<byte> spanBody);

        protected virtual Span<byte> FillBlockBaseHeader(BlockBase blockBase, Span<byte> spanHeader)
        {
            PacketType packetType = (PacketType)BinaryPrimitives.ReadUInt16LittleEndian(spanHeader);

            return spanHeader.Slice(2);
        }
    }
}
