﻿using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.HashCalculations;
using Wist.Core.Identity;

namespace Wist.BlockLattice.Core.Parsers.Registry
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
    public class RegistryShortBlockParser : SyncedBlockParserBase
    {
        private readonly IIdentityKeyProvider _transactionHashKeyProvider;
        public RegistryShortBlockParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, IHashCalculationsRepository proofOfWorkCalculationRepository) : base(identityKeyProvidersRegistry, proofOfWorkCalculationRepository)
        {
            _transactionHashKeyProvider = identityKeyProvidersRegistry.GetInstance("TransactionRegistry");
        }

        public override ushort BlockType => BlockTypes.Registry_ShortBlock;

        public override PacketType PacketType => PacketType.Registry;

        protected override Span<byte> ParseSynced(ushort version, Span<byte> spanBody, out SyncedBlockBase syncedBlockBase)
        {
            RegistryShortBlock transactionsShortBlock = new RegistryShortBlock();
            ushort itemsCount = BinaryPrimitives.ReadUInt16LittleEndian(spanBody);

            transactionsShortBlock.TransactionHeaderHashes = new SortedList<ushort, IKey>(itemsCount);

            for (int i = 0; i < itemsCount; i++)
            {
                ushort order = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Slice(3 + i * (Globals.POW_HASH_SIZE + 2)));
                byte[] hashKey = spanBody.Slice(2 + i * (Globals.POW_HASH_SIZE + 2) + 2, Globals.POW_HASH_SIZE).ToArray();

                IKey key = _transactionHashKeyProvider.GetKey(hashKey);

                transactionsShortBlock.TransactionHeaderHashes.Add(order, key);
            }

            syncedBlockBase = transactionsShortBlock;

            return spanBody.Slice(2 + itemsCount * (Globals.POW_HASH_SIZE + 2));
        }
    }
}
