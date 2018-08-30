using System;
using System.Buffers.Binary;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Identity;
using Wist.Core.HashCalculations;

namespace Wist.BlockLattice.Core.Parsers.Registry
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionRegisterBlockParser : SyncedBlockParserBase
    {
        public TransactionRegisterBlockParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, IHashCalculationsRepository proofOfWorkCalculationRepository) : base(identityKeyProvidersRegistry, proofOfWorkCalculationRepository)
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
            byte[] referencedTargetHash = spanBody.Slice(12 + Globals.HASH_SIZE, Globals.HASH_SIZE).ToArray();
            TransactionRegisterBlock transactionRegisterBlock = new TransactionRegisterBlock
            {
                TransactionHeader = new TransactionHeader
                {
                    ReferencedPacketType = referencedPacketType,
                    ReferencedBlockType = referencedBlockType,
                    ReferencedHeight = referencedBlockHeight,
                    ReferencedBodyHash = referencedBlockHash,
                    ReferencedTargetHash = referencedTargetHash
                }
            };

            syncedBlockBase = transactionRegisterBlock;

            return spanBody.Slice(12 + Globals.HASH_SIZE + Globals.HASH_SIZE);
        }
    }
}
