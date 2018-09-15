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

        protected override Span<byte> ParseSigned(ushort version, Span<byte> spanBody, out SignedBlockBase signedBlockBase)
        {
            ulong blockHeight = BinaryPrimitives.ReadUInt64LittleEndian(spanBody);
            SyncedBlockBase syncedBlockBase;
            Span<byte> spanPostBody = ParseSynced(version, spanBody.Slice(8), out syncedBlockBase);
            syncedBlockBase.BlockHeight = blockHeight;
            signedBlockBase = syncedBlockBase;

            return spanPostBody;
        }

        protected override Span<byte> SliceInitialBytes(Span<byte> span, out Span<byte> spanHeader)
        {
            Span<byte> span1 = base.SliceInitialBytes(span, out spanHeader);

            spanHeader = span.Slice(0, spanHeader.Length + 8 + Globals.NONCE_LENGTH + Globals.POW_HASH_SIZE);

            return span1.Slice(8 + Globals.NONCE_LENGTH + Globals.POW_HASH_SIZE);
        }

        protected override Span<byte> FillBlockBaseHeader(BlockBase blockBase, Span<byte> spanHeader)
        {
            SyncedBlockBase syncedBlockBase = (SyncedBlockBase)blockBase;

            spanHeader = base.FillBlockBaseHeader(blockBase, spanHeader);

            syncedBlockBase.SyncBlockHeight = BinaryPrimitives.ReadUInt64LittleEndian(spanHeader);
            syncedBlockBase.Nonce = BinaryPrimitives.ReadUInt32LittleEndian(spanHeader.Slice(8));
            syncedBlockBase.PowHash = spanHeader.Slice(12, Globals.POW_HASH_SIZE).ToArray();

            return spanHeader.Slice(12 + Globals.POW_HASH_SIZE);
        }

        protected abstract Span<byte> ParseSynced(ushort version, Span<byte> spanBody, out SyncedBlockBase syncedBlockBase);
    }
}
