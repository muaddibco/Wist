using System.IO;
using Wist.BlockLattice.Core.DataModel.Transactional;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.HashCalculations;
using Wist.Core.Identity;

namespace Wist.BlockLattice.Core.Serializers.Signed.Transactional
{
    [RegisterExtension(typeof(ISignatureSupportSerializer), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class TransferFundsBlockSerializer : SyncLinkedSupportSerializerBase<TransferFundsBlock>
    {
        public TransferFundsBlockSerializer(ICryptoService cryptoService, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, 
            IHashCalculationsRepository hashCalculationsRepository) 
            : base(PacketType.TransactionalChain, BlockTypes.Transaction_TransferFunds, cryptoService, identityKeyProvidersRegistry, hashCalculationsRepository)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            base.WriteBody(bw);

            bw.Write(_block.UptodateFunds);
            bw.Write(_block.TargetOriginalHash);
        }
    }
}
