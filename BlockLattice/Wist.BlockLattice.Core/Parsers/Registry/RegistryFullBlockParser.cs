using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.HashCalculations;
using Wist.Core.Identity;

namespace Wist.BlockLattice.Core.Parsers.Registry
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
    public class RegistryFullBlockParser : SyncedBlockParserBase
    {
        private readonly RegistryRegisterBlockParser _registryRegisterBlockParser;

        public RegistryFullBlockParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, IHashCalculationsRepository hashCalculationRepository) : base(identityKeyProvidersRegistry, hashCalculationRepository)
        {
            _registryRegisterBlockParser = new RegistryRegisterBlockParser(identityKeyProvidersRegistry, hashCalculationRepository);
        }

        public override ushort BlockType => BlockTypes.Registry_FullBlock;

        public override PacketType PacketType => PacketType.Registry;

        protected override Span<byte> ParseSynced(ushort version, Span<byte> spanBody, out SyncedBlockBase syncedBlockBase)
        {
            RegistryFullBlock transactionsFullBlock = new RegistryFullBlock();
            ushort itemsCount = BinaryPrimitives.ReadUInt16LittleEndian(spanBody);

            transactionsFullBlock.TransactionHeaders = new SortedList<ushort, RegistryRegisterBlock>(itemsCount);

            int registryRegisterPacketSize = ((spanBody.Length - 2 - Globals.NODE_PUBLIC_KEY_SIZE - Globals.SIGNATURE_SIZE) / itemsCount) - 2;

            for (int i = 0; i < itemsCount; i++)
            {
                ushort order = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Slice(3 + i * (Globals.POW_HASH_SIZE + 2)));
                byte[] registryRegisterPacket = spanBody.Slice(2 + i * (registryRegisterPacketSize + 2) + 2, registryRegisterPacketSize).ToArray();

                RegistryRegisterBlock registryRegisterBlock = (RegistryRegisterBlock)_registryRegisterBlockParser.Parse(registryRegisterPacket);

                transactionsFullBlock.TransactionHeaders.Add(order, registryRegisterBlock);
            }

            syncedBlockBase = transactionsFullBlock;

            return spanBody.Slice(2 + itemsCount * (registryRegisterPacketSize + 2));
        }
    }
}
