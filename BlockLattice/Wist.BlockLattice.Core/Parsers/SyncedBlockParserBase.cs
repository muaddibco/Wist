using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.Core.Identity;
using Wist.Core.ProofOfWork;

namespace Wist.BlockLattice.Core.Parsers
{
    public abstract class SyncedBlockParserBase : SignedBlockParserBase
    {
        private readonly IProofOfWorkCalculationRepository _proofOfWorkCalculationRepository;

        public SyncedBlockParserBase(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, IProofOfWorkCalculationRepository proofOfWorkCalculationRepository)
            : base(identityKeyProvidersRegistry)
        {
            _proofOfWorkCalculationRepository = proofOfWorkCalculationRepository;
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

            POWType powType = (POWType)BinaryPrimitives.ReadUInt16LittleEndian(span1.Slice(8));
            int sliceSkip = 0;

            if (powType != POWType.None)
            {
                IProofOfWorkCalculation proofOfWorkCalculation = _proofOfWorkCalculationRepository.Create(powType);
                sliceSkip = 8 + proofOfWorkCalculation.HashSize;
                _proofOfWorkCalculationRepository.Utilize(proofOfWorkCalculation);
            }

            spanHeader = span.Slice(0, spanHeader.Length + 10 + sliceSkip);

            return span1.Slice(10 + sliceSkip);
        }

        protected override Span<byte> FillBlockBaseHeader(BlockBase blockBase, Span<byte> spanHeader)
        {
            SyncedBlockBase syncedBlockBase = (SyncedBlockBase)blockBase;

            spanHeader = base.FillBlockBaseHeader(blockBase, spanHeader);

            syncedBlockBase.SyncBlockOrder = BinaryPrimitives.ReadUInt64LittleEndian(spanHeader);
            POWType powType = (POWType)BinaryPrimitives.ReadUInt16LittleEndian(spanHeader.Slice(8));

            if (powType != POWType.None)
            {
                IProofOfWorkCalculation proofOfWorkCalculation = _proofOfWorkCalculationRepository.Create(powType);
                int hashSize = proofOfWorkCalculation.HashSize;
                syncedBlockBase.Nonce = BinaryPrimitives.ReadUInt64LittleEndian(spanHeader.Slice(10));
                syncedBlockBase.HashNonce = spanHeader.Slice(18, hashSize).ToArray();
                _proofOfWorkCalculationRepository.Utilize(proofOfWorkCalculation);

                return spanHeader.Slice(18 + hashSize);
            }

            return spanHeader.Slice(10);
        }

        protected abstract Span<byte> ParseSynced(ushort version, Span<byte> spanBody, out SyncedBlockBase syncedBlockBase);
    }
}
