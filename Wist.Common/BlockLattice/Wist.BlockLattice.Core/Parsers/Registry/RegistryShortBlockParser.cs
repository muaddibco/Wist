using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Exceptions;
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
            _transactionHashKeyProvider = identityKeyProvidersRegistry.GetTransactionsIdenityKeyProvider();
        }

        public override ushort BlockType => BlockTypes.Registry_ShortBlock;

        public override PacketType PacketType => PacketType.Registry;

        protected override Memory<byte> ParseSynced(ushort version, Memory<byte> spanBody, out SyncedBlockBase syncedBlockBase)
        {
            if (version == 1)
            {
                RegistryShortBlock transactionsShortBlock = new RegistryShortBlock();
                ushort itemsCount = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Span);

                transactionsShortBlock.TransactionHeaderHashes = new SortedList<ushort, IKey>(itemsCount);

                for (int i = 0; i < itemsCount; i++)
                {
                    ushort order = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Span.Slice(2 + i * (Globals.TRANSACTION_KEY_HASH_SIZE + 2)));
                    byte[] hashKey = spanBody.Slice(2 + i * (Globals.TRANSACTION_KEY_HASH_SIZE + 2) + 2, Globals.TRANSACTION_KEY_HASH_SIZE).ToArray();

                    IKey key = _transactionHashKeyProvider.GetKey(hashKey);

                    transactionsShortBlock.TransactionHeaderHashes.Add(order, key);
                }

                syncedBlockBase = transactionsShortBlock;

                return spanBody.Slice(2 + itemsCount * (Globals.TRANSACTION_KEY_HASH_SIZE + 2));
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}
