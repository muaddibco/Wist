﻿using System.IO;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;

namespace Wist.BlockLattice.Core.Serializers.Signed.Registry
{
    [RegisterExtension(typeof(ISignatureSupportSerializer), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class RegistryConfidenceBlockSerializer : SyncSupportSerializerBase<RegistryConfidenceBlock>
    {
        public RegistryConfidenceBlockSerializer(PacketType packetType, ushort blockType, ICryptoService cryptoService) : base(packetType, blockType, cryptoService)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            base.WriteBody(bw);

            bw.Write(_block.Confidence);
            bw.Write(_block.ReferencedBlockHash);
        }
    }
}