using System.IO;
using Wist.BlockLattice.Core.DataModel.Synchronization;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Exceptions;
using Wist.Core.Identity;
using Wist.Core.ProofOfWork;

namespace Wist.BlockLattice.Core.Parsers.Synchronization
{
    public abstract class SynchronizationConfirmedBlockParser : SynchronizationBlockParserBase
    {
        public SynchronizationConfirmedBlockParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) : base(identityKeyProvidersRegistry)
        {
        }

        public override ushort BlockType => BlockTypes.Synchronization_ConfirmedBlock;

        protected override SynchronizationBlockBase ParseSynchronization(ushort version, BinaryReader br)
        {
            SynchronizationBlockBase synchronizationConfirmedBlock;

            if(version == 1)
            {
                ushort round = br.ReadUInt16();
                byte numberOfSigners = br.ReadByte();
                byte[][] signers = new byte[numberOfSigners][];
                byte[][] signatures = new byte[numberOfSigners][];

                for (int i = 0; i < numberOfSigners; i++)
                {
                    signers[i] = br.ReadBytes(Globals.NODE_PUBLIC_KEY_SIZE);
                    signatures[i] = br.ReadBytes(Globals.SIGNATURE_SIZE);
                }

                synchronizationConfirmedBlock = new SynchronizationConfirmedBlock()
                {
                    Round = round,
                    PublicKeys = signers,
                    Signatures = signatures
                };
            }
            else
            {
                throw new BlockVersionNotSupportedException(version, BlockType);
            }

            return synchronizationConfirmedBlock;
        }
    }
}
