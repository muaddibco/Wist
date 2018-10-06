﻿using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Exceptions;
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

        protected override Memory<byte> ParseSynced(ushort version, Memory<byte> spanBody, out SyncedBlockBase syncedBlockBase)
        {
            if (version == 1)
            {
                RegistryFullBlock transactionsFullBlock = new RegistryFullBlock();
                ushort itemsCount = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Span);

                transactionsFullBlock.TransactionHeaders = new SortedList<ushort, RegistryRegisterBlock>(itemsCount);
                int registryRegisterPacketSize = 0;

                if (itemsCount > 0)
                {
                    registryRegisterPacketSize = ((spanBody.Length - 2 - Globals.DEFAULT_HASH_SIZE - Globals.NODE_PUBLIC_KEY_SIZE - Globals.SIGNATURE_SIZE) / itemsCount) - 2;

                    for (int i = 0; i < itemsCount; i++)
                    {
                        ushort order = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Span.Slice(2 + i * (registryRegisterPacketSize + 2)));
                        byte[] registryRegisterPacket = spanBody.Slice(2 + i * (registryRegisterPacketSize + 2) + 2, registryRegisterPacketSize).ToArray();

                        RegistryRegisterBlock registryRegisterBlock = (RegistryRegisterBlock)_registryRegisterBlockParser.Parse(registryRegisterPacket);

                        transactionsFullBlock.TransactionHeaders.Add(order, registryRegisterBlock);
                    }
                }

                transactionsFullBlock.ShortBlockHash = spanBody.Slice(2 + itemsCount * (registryRegisterPacketSize + 2), Globals.DEFAULT_HASH_SIZE).ToArray();

                syncedBlockBase = transactionsFullBlock;

                return spanBody.Slice(2 + itemsCount * (registryRegisterPacketSize + 2) + Globals.DEFAULT_HASH_SIZE);
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}
