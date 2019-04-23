using System;
using System.Buffers.Binary;
using Wist.Blockchain.Core.DataModel.Synchronization;
using Wist.Blockchain.Core.Enums;
using Wist.Blockchain.Core.Exceptions;
using Wist.Core;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Identity;

namespace Wist.Blockchain.Core.Parsers.Synchronization
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationConfirmedBlockParser : SynchronizationBlockParserBase
    {
        public SynchronizationConfirmedBlockParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) 
            : base(identityKeyProvidersRegistry)
        {
        }

        public override ushort BlockType => BlockTypes.Synchronization_ConfirmedBlock;

        protected override Memory<byte> ParseSynchronization(ushort version, Memory<byte> spanBody, out SynchronizationBlockBase synchronizationBlockBase)
        {
            if(version == 1)
            {
                ushort round = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Span);
                byte numberOfSigners = spanBody.Span[2];
                byte[][] signers = new byte[numberOfSigners][];
                byte[][] signatures = new byte[numberOfSigners][];

                for (int i = 0; i < numberOfSigners; i++)
                {
                    signers[i] = spanBody.Slice(3 + (Globals.NODE_PUBLIC_KEY_SIZE + Globals.SIGNATURE_SIZE)* i, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                    signatures[i] = spanBody.Slice(3 + (Globals.NODE_PUBLIC_KEY_SIZE + Globals.SIGNATURE_SIZE )* i + Globals.NODE_PUBLIC_KEY_SIZE, Globals.SIGNATURE_SIZE).ToArray();
                }

                SynchronizationBlockBase synchronizationConfirmedBlock = new SynchronizationConfirmedBlock()
                {
                    Round = round,
                    PublicKeys = signers,
                    Signatures = signatures
                };

                synchronizationBlockBase = synchronizationConfirmedBlock;

                return spanBody.Slice(3 + (Globals.NODE_PUBLIC_KEY_SIZE + Globals.SIGNATURE_SIZE) * numberOfSigners);
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}
