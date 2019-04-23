using System;
using System.Buffers.Binary;
using Wist.Blockchain.Core.Enums;
using Wist.Core;
using Wist.Core.Identity;
using Wist.Core.Models;

namespace Wist.Blockchain.Core.Parsers
{
    public abstract class BlockParserBase : IBlockParser
    {
        protected readonly IIdentityKeyProvider _entityIdentityKeyProvider;

        public BlockParserBase(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry)
        {
            _entityIdentityKeyProvider = identityKeyProvidersRegistry.GetInstance();
        }

        public abstract ushort BlockType { get; }

        public abstract PacketType PacketType { get; }

        public virtual PacketBase Parse(Memory<byte> source)
        {
            //Memory<byte> sourceMemory = new Memory<byte>(source);

            Memory<byte> spanBody = SliceInitialBytes(source, out Memory<byte> spanHeader);

            PacketBase blockBase = ParseBody(spanBody, out Memory<byte> nonHeaderBytes);
			blockBase.BodyBytes = GetBodyBytes(source.Slice(0, spanHeader.Length + nonHeaderBytes.Length));

			blockBase.RawData = source.Slice(0, spanHeader.Length + nonHeaderBytes.Length);

            FillBlockBaseHeader(blockBase, spanHeader);

            return blockBase;
        }

        private PacketBase ParseBody(Memory<byte> spanBody, out Memory<byte> nonHeaderBytes)
        {
            ushort version = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Span);
            ushort messageType = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Span.Slice(2));
            PacketBase blockBase = ParseBlockBase(version, spanBody.Slice(4), out Memory<byte> spanPostBody);

            nonHeaderBytes = spanBody.Slice(0, spanBody.Length - spanPostBody.Length);

            return blockBase;
        }

        protected virtual Memory<byte> GetBodyBytes(Memory<byte> spanBody)
        {
            return spanBody;
        }

        protected virtual Memory<byte> SliceInitialBytes(Memory<byte> span, out Memory<byte> spanHeader)
        {
            spanHeader = span.Slice(0, 2 + Globals.SYNC_BLOCK_HEIGHT_LENGTH + Globals.NONCE_LENGTH + Globals.POW_HASH_SIZE);

            return span.Slice(2 + Globals.SYNC_BLOCK_HEIGHT_LENGTH + Globals.NONCE_LENGTH + Globals.POW_HASH_SIZE);
        }

        protected abstract PacketBase ParseBlockBase(ushort version, Memory<byte> spanBody, out Memory<byte> spanPostBody);

        protected virtual Memory<byte> FillBlockBaseHeader(PacketBase blockBase, Memory<byte> spanHeader)
        {
            PacketType packetType = (PacketType)BinaryPrimitives.ReadUInt16LittleEndian(spanHeader.Span);
            int readBytes = sizeof(ushort);

            blockBase.SyncBlockHeight = BinaryPrimitives.ReadUInt64LittleEndian(spanHeader.Slice(readBytes).Span);
            readBytes += sizeof(ulong);

            blockBase.Nonce = BinaryPrimitives.ReadUInt32LittleEndian(spanHeader.Slice(readBytes).Span);
            readBytes += sizeof(uint);

            blockBase.PowHash = spanHeader.Slice(readBytes, Globals.POW_HASH_SIZE).ToArray();
            readBytes += Globals.POW_HASH_SIZE;

            return spanHeader.Slice(readBytes);
        }

        public static void GetPacketAndBlockTypes(Memory<byte> source, out PacketType packetType, out ushort blockType)
        {
            int blockTypePos = Globals.PACKET_TYPE_LENGTH + Globals.SYNC_BLOCK_HEIGHT_LENGTH + Globals.NONCE_LENGTH + Globals.POW_HASH_SIZE + Globals.VERSION_LENGTH;
            packetType = (PacketType)BinaryPrimitives.ReadUInt16LittleEndian(source.Span);
            blockType = BinaryPrimitives.ReadUInt16LittleEndian(source.Span.Slice(blockTypePos));
        }
    }
}
