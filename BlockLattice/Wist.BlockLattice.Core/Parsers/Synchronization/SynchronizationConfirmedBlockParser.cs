using System;
using System.Buffers.Binary;
using System.IO;
using Wist.BlockLattice.Core.DataModel.Synchronization;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Exceptions;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Identity;
using Wist.Core.ProofOfWork;

namespace Wist.BlockLattice.Core.Parsers.Synchronization
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationConfirmedBlockParser : SynchronizationBlockParserBase
    {
        public SynchronizationConfirmedBlockParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, IProofOfWorkCalculationRepository proofOfWorkCalculationRepository) 
            : base(identityKeyProvidersRegistry, proofOfWorkCalculationRepository)
        {
        }

        public override ushort BlockType => BlockTypes.Synchronization_ConfirmedBlock;

        protected override Span<byte> ParseSynchronization(ushort version, Span<byte> spanBody, out SynchronizationBlockBase synchronizationBlockBase)
        {
            SynchronizationBlockBase synchronizationConfirmedBlock;

            if(version == 1)
            {
                ushort round = BinaryPrimitives.ReadUInt16LittleEndian(spanBody);
                byte numberOfSigners = spanBody[2];
                byte[][] signers = new byte[numberOfSigners][];
                byte[][] signatures = new byte[numberOfSigners][];

                for (int i = 0; i < numberOfSigners; i++)
                {
                    signers[i] = spanBody.Slice(3 + Globals.NODE_PUBLIC_KEY_SIZE * i, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                    signatures[i] = spanBody.Slice(3 + Globals.NODE_PUBLIC_KEY_SIZE * (i + 1) + Globals.SIGNATURE_SIZE * i, Globals.SIGNATURE_SIZE).ToArray();
                }

                synchronizationConfirmedBlock = new SynchronizationConfirmedBlock()
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
