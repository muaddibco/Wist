using CommonServiceLocator;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using Wist.Blockchain.Core.DataModel;
using Wist.Blockchain.Core.DataModel.Registry;
using Wist.Blockchain.Core.Enums;
using Wist.Blockchain.Core.Exceptions;
using Wist.Core;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.HashCalculations;
using Wist.Core.Identity;
using Wist.Core.Models;

namespace Wist.Blockchain.Core.Parsers.Registry
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
    public class RegistryFullBlockParser : SignedBlockParserBase
    {
        private readonly RegistryRegisterBlockParser _registryRegisterBlockParser;
        private readonly RegistryRegisterUtxoConfidentialBlockParser _registryRegisterUtxoConfidentialBlockParser;

        public RegistryFullBlockParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) : base(identityKeyProvidersRegistry)
        {
            _registryRegisterBlockParser = new RegistryRegisterBlockParser(identityKeyProvidersRegistry);
            _registryRegisterUtxoConfidentialBlockParser = new RegistryRegisterUtxoConfidentialBlockParser(identityKeyProvidersRegistry);
        }

        public override ushort BlockType => BlockTypes.Registry_FullBlock;

        public override PacketType PacketType => PacketType.Registry;

        protected override Memory<byte> ParseSigned(ushort version, Memory<byte> spanBody, out SignedPacketBase syncedBlockBase)
        {
            if (version == 1)
            {
                int readBytes = 0;

                RegistryFullBlock transactionsFullBlock = new RegistryFullBlock();
                ushort stateWitnessesCount = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Span.Slice(readBytes));
                readBytes += sizeof(ushort);

                ushort utxoWitnessesCount = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Span.Slice(readBytes));
                readBytes += sizeof(ushort);

                transactionsFullBlock.StateWitnesses = new RegistryRegisterBlock[stateWitnessesCount];
                transactionsFullBlock.UtxoWitnesses = new RegistryRegisterUtxoConfidential[utxoWitnessesCount];

                for (int i = 0; i < stateWitnessesCount; i++)
                {
                    RegistryRegisterBlock block = (RegistryRegisterBlock)_registryRegisterBlockParser.Parse(spanBody.Slice(readBytes));

                    readBytes += block?.RawData.Length ?? 0;

                    transactionsFullBlock.StateWitnesses[i] = block;
                }

                for (int i = 0; i < utxoWitnessesCount; i++)
                {
                    RegistryRegisterUtxoConfidential block = (RegistryRegisterUtxoConfidential)_registryRegisterUtxoConfidentialBlockParser.Parse(spanBody.Slice(readBytes));

                    readBytes += block?.RawData.Length ?? 0;

                    transactionsFullBlock.UtxoWitnesses[i] = block;
                }

                transactionsFullBlock.ShortBlockHash = spanBody.Slice(readBytes, Globals.DEFAULT_HASH_SIZE).ToArray();
                readBytes += Globals.DEFAULT_HASH_SIZE;

                syncedBlockBase = transactionsFullBlock;

                return spanBody.Slice(readBytes);
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}
