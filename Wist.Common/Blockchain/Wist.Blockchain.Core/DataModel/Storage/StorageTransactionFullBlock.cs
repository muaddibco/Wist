﻿using Wist.Blockchain.Core.Enums;

namespace Wist.Blockchain.Core.DataModel.Storage
{
    public class StorageTransactionFullBlock : StorageBlockBase
    {
        public override ushort BlockType => BlockTypes.Storage_TransactionFull;

        public override ushort Version => 1;

        public byte[] Transaction { get; set; }
    }
}
