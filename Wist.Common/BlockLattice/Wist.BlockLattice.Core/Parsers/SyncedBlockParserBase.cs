using System;
using System.Buffers.Binary;
using Wist.BlockLattice.Core.DataModel;
using Wist.Core.Identity;
using Wist.Core.HashCalculations;

namespace Wist.BlockLattice.Core.Parsers
{
    public abstract class SyncedBlockParserBase : SignedBlockParserBase
    {
        protected readonly IHashCalculationsRepository _hashCalculationRepository;

        public SyncedBlockParserBase(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, IHashCalculationsRepository hashCalculationRepository)
            : base(identityKeyProvidersRegistry)
        {
            _hashCalculationRepository = hashCalculationRepository;
        }

        protected override Memory<byte> ParseSigned(ushort version, Memory<byte> spanBody, out SignedBlockBase signedBlockBase)
        {
            ulong blockHeight = BinaryPrimitives.ReadUInt64LittleEndian(spanBody.Span);
            SyncedBlockBase syncedBlockBase;
            Memory<byte> spanPostBody = ParseSynced(version, spanBody.Slice(8), out syncedBlockBase);
            syncedBlockBase.BlockHeight = blockHeight;
            signedBlockBase = syncedBlockBase;

            return spanPostBody;
        }

        protected override Memory<byte> SliceInitialBytes(Memory<byte> span, out Memory<byte> spanHeader)
        {
            Memory<byte> span1 = base.SliceInitialBytes(span, out spanHeader);

            spanHeader = span.Slice(0, spanHeader.Length + 8 + Globals.NONCE_LENGTH + Globals.POW_HASH_SIZE);

            return span1.Slice(8 + Globals.NONCE_LENGTH + Globals.POW_HASH_SIZE);
        }

        protected override Memory<byte> FillBlockBaseHeader(BlockBase blockBase, Memory<byte> spanHeader)
        {
            SyncedBlockBase syncedBlockBase = (SyncedBlockBase)blockBase;

            spanHeader = base.FillBlockBaseHeader(blockBase, spanHeader);

            syncedBlockBase.SyncBlockHeight = BinaryPrimitives.ReadUInt64LittleEndian(spanHeader.Span);
            syncedBlockBase.Nonce = BinaryPrimitives.ReadUInt32LittleEndian(spanHeader.Slice(8).Span);
            syncedBlockBase.PowHash = spanHeader.Slice(12, Globals.POW_HASH_SIZE).ToArray();

            return spanHeader.Slice(12 + Globals.POW_HASH_SIZE);
        }

        protected abstract Memory<byte> ParseSynced(ushort version, Memory<byte> spanBody, out SyncedBlockBase syncedBlockBase);
    }
}
