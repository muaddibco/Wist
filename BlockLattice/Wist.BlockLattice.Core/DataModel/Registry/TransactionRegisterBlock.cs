using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel.Registry
{
    public class TransactionRegisterBlock : RegistryBlockBase, IEqualityComparer<TransactionRegisterBlock>
    {
        public override ushort BlockType => BlockTypes.Registry_TransactionRegister;

        public override ushort Version => 1;

        public TransactionHeader TransactionHeader { get; set; }

        public bool Equals(TransactionRegisterBlock x, TransactionRegisterBlock y)
        {
            if(x != null && y != null)
            {
                return x.PacketType == y.PacketType && x.BlockType == y.BlockType && x.TransactionHeader.Equals(y.TransactionHeader);
            }

            return x == null && y == null;
        }

        public int GetHashCode(TransactionRegisterBlock obj)
        {
            return obj.PacketType.GetHashCode() ^ obj.BlockType.GetHashCode() ^ obj.TransactionHeader.GetHashCode(); 
        }
    }
}
