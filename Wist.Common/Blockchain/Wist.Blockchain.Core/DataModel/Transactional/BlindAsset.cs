using System;
using System.Collections.Generic;
using System.Text;
using Wist.Blockchain.Core.DataModel.Transactional.Internal;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Cryptography;

namespace Wist.Blockchain.Core.DataModel.Transactional
{
    public class BlindAsset : TransactionalPacketBase
    {
        public override ushort Version => 1;

        public override ushort BlockType => BlockTypes.Transaction_BlindAsset;

        public EncryptedAsset EncryptedAsset { get; set; }

        public SurjectionProof SurjectionProof { get; set; }
    }
}
