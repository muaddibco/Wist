using System;
using System.Buffers.Binary;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Identity;
using Wist.Core.ProofOfWork;

namespace Wist.BlockLattice.Core.Parsers.Registry
{
    public class TransactionRegisterBlockParser : SyncedBlockParserBase
    {
        public TransactionRegisterBlockParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, IProofOfWorkCalculationRepository proofOfWorkCalculationRepository) : base(identityKeyProvidersRegistry, proofOfWorkCalculationRepository)
        {
        }

        public override ushort BlockType => BlockTypes.Registry_TransactionRegister;

        public override PacketType PacketType => PacketType.Registry;

        protected override Span<byte> ParseSynced(ushort version, Span<byte> spanBody, out SyncedBlockBase syncedBlockBase)
        {
            PacketType referencedPacketType = (PacketType)BinaryPrimitives.ReadUInt16LittleEndian(spanBody);
            ushort referencedBlockType = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Slice(2));
            ulong referencedBlockHeight = BinaryPrimitives.ReadUInt64LittleEndian(spanBody.Slice(4));
            byte[] referencedBlockHash = spanBody.Slice(12, Globals.HASH_SIZE).ToArray();
            TransactionRegisterBlock transactionRegisterBlock = new TransactionRegisterBlock
            {
                ReferencedPacketType = referencedPacketType,
                ReferencedBlockType = referencedBlockType,
                ReferencedHeight = referencedBlockHeight,
                ReferencedBodyHash = referencedBlockHash
            };

            syncedBlockBase = transactionRegisterBlock;

            return spanBody.Slice(12 + Globals.HASH_SIZE);
        }
    }
}
