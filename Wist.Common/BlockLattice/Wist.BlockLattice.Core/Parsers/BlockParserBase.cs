using System;
using System.Buffers.Binary;
using System.IO;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.ExtensionMethods;
using Wist.Core.HashCalculations;
using Wist.Core.Identity;

namespace Wist.BlockLattice.Core.Parsers
{
    public abstract class BlockParserBase : IBlockParser
    {
        protected readonly IIdentityKeyProvider _entityIdentityKeyProvider;

        public BlockParserBase(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry)
        {
        }

        public abstract ushort BlockType { get; }

        public abstract PacketType PacketType { get; }

        public virtual BlockBase ParseBody(byte[] blockBody)
        {
            Memory<byte> span = new Memory<byte>(blockBody);

            BlockBase blockBase = ParseBody(span);

            return blockBase;
        }

        public virtual BlockBase Parse(byte[] source)
        {
            Memory<byte> sourceMemory = new Memory<byte>(source);

            Memory<byte> spanBody = SliceInitialBytes(sourceMemory, out Memory<byte> spanHeader);

            BlockBase blockBase = ParseBody(spanBody);

            blockBase.RawData = sourceMemory;

            FillBlockBaseHeader(blockBase, spanHeader);

            return blockBase;
        }

        private BlockBase ParseBody(Memory<byte> spanBody)
        {
            ushort version = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Span);
            ushort messageType = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Span.Slice(2));
            BlockBase blockBase = ParseBlockBase(version, spanBody.Slice(4));

            blockBase.BodyBytes = GetBodyBytes(spanBody);
            blockBase.NonHeaderBytes = spanBody;

            return blockBase;
        }

        protected virtual Memory<byte> GetBodyBytes(Memory<byte> spanBody)
        {
            return spanBody;
        }

        protected virtual Memory<byte> SliceInitialBytes(Memory<byte> span, out Memory<byte> spanHeader)
        {
            spanHeader = span.Slice(0, 2);

            return span.Slice(2);
        }

        protected abstract BlockBase ParseBlockBase(ushort version, Memory<byte> spanBody);

        protected virtual Memory<byte> FillBlockBaseHeader(BlockBase blockBase, Memory<byte> spanHeader)
        {
            PacketType packetType = (PacketType)BinaryPrimitives.ReadUInt16LittleEndian(spanHeader.Span);

            return spanHeader.Slice(2);
        }
    }
}
