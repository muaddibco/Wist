using System;
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
    public class TransactionsRegistryShortBlockParser : SyncedBlockParserBase
    {
        private readonly IIdentityKeyProvider _transactionHashKeyProvider;
        public TransactionsRegistryShortBlockParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, IHashCalculationRepository proofOfWorkCalculationRepository) : base(identityKeyProvidersRegistry, proofOfWorkCalculationRepository)
        {
            _transactionHashKeyProvider = identityKeyProvidersRegistry.GetInstance("TransactionRegistry");
        }

        public override ushort BlockType => BlockTypes.Registry_TransactionShortBlock;

        public override PacketType PacketType => PacketType.Registry;

        protected override Span<byte> ParseSynced(ushort version, Span<byte> spanBody, out SyncedBlockBase syncedBlockBase)
        {
            TransactionsShortBlock transactionsShortBlock = new TransactionsShortBlock();
            byte round = spanBody[0];
            ushort itemsCount = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Slice(1));

            transactionsShortBlock.Round = round;
            transactionsShortBlock.TransactionHeaderHashes = new SortedList<ushort, IKey>(itemsCount);

            for (int i = 0; i < itemsCount; i++)
            {
                ushort order = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Slice(3 + i * (Globals.POW_HASH_SIZE + 2)));
                byte[] hashKey = spanBody.Slice(3 + i * 26 + 2, Globals.POW_HASH_SIZE).ToArray();

                IKey key = _transactionHashKeyProvider.GetKey(hashKey);

                transactionsShortBlock.TransactionHeaderHashes.Add(order, key);
            }

            syncedBlockBase = transactionsShortBlock;

            return spanBody.Slice(3 + itemsCount * (Globals.POW_HASH_SIZE + 2));
        }
    }
}
