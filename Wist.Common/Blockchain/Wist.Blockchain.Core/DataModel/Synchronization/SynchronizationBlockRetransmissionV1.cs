﻿using System;
using System.Collections.Generic;
using System.Text;
using Wist.Blockchain.Core.Enums;

namespace Wist.Blockchain.Core.DataModel.Synchronization
{
    public class SynchronizationBlockRetransmissionV1 : SynchronizationBlockBase
    {
        public override ushort BlockType => BlockTypes.Synchronization_RetransmissionBlock;

        public override ushort Version => 1;

        public ushort OffsetSinceLastMedian { get; set; }

        public byte[] ConfirmationSignature { get; set; }

        public byte[] ConfirmationPublicKey { get; set; }

    }
}
