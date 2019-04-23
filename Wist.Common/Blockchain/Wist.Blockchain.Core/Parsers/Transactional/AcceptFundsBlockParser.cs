using System;
using System.Buffers.Binary;
using System.IO;
using Wist.Blockchain.Core.DataModel.Transactional;
using Wist.Blockchain.Core.Enums;
using Wist.Blockchain.Core.Exceptions;
using Wist.Blockchain.Core.Interfaces;
using Wist.Core;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Identity;

namespace Wist.Blockchain.Core.Parsers.Transactional
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
    public class AcceptFundsBlockParser : TransactionalBlockParserBase
    {
        public AcceptFundsBlockParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) : base(identityKeyProvidersRegistry)
        {
        }

        public override ushort BlockType => BlockTypes.Transaction_AcceptFunds;

        protected override Memory<byte> ParseTransactional(ushort version, Memory<byte> spanBody, out TransactionalPacketBase transactionalBlockBase)
        {
            TransactionalPacketBase block = null;

            if (version == 1)
            {
                byte[] origin = spanBody.Slice(0, Globals.DEFAULT_HASH_SIZE).ToArray();
                ulong funds = BinaryPrimitives.ReadUInt64LittleEndian(spanBody.Span.Slice(Globals.DEFAULT_HASH_SIZE));

                block = new AcceptFundsBlock()
                {
                    SourceOriginalHash = origin,
                    UptodateFunds = funds,
                };

                transactionalBlockBase = block;

                return spanBody.Slice(Globals.DEFAULT_HASH_SIZE + 8);
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}
