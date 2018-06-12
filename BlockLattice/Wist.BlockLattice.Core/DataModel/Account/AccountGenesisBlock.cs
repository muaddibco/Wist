using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel.Account
{
    public class AccountGenesisBlock : GenesisBlockBase
    {
        public override PacketType ChainType => PacketType.AccountChain;

        public override ushort Version => 1;

        /// <summary>
        /// 32-byte length Public Key
        /// </summary>
        public byte[] PublicKey { get; set; }
    }
}
