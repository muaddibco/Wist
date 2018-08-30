﻿using System;
using System.Collections.Generic;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Cryptography;
using Wist.Core.Identity;

namespace Wist.BlockLattice.Core.DataModel.Registry
{
    public class RegistryRegisterBlock : RegistryBlockBase, IEqualityComparer<RegistryRegisterBlock>
    {
        public override ushort BlockType => BlockTypes.Registry_Register;

        public override ushort Version => 1;

        public TransactionHeader TransactionHeader { get; set; }

        public bool Equals(RegistryRegisterBlock x, RegistryRegisterBlock y)
        {
            if(x != null && y != null)
            {
                return x.PacketType == y.PacketType && x.BlockType == y.BlockType && x.TransactionHeader.Equals(y.TransactionHeader);
            }

            return x == null && y == null;
        }

        public int GetHashCode(RegistryRegisterBlock obj)
        {
            return obj.PacketType.GetHashCode() ^ obj.BlockType.GetHashCode() ^ obj.TransactionHeader.GetHashCode(); 
        }

        public override int GetHashCode()
        {
            return PacketType.GetHashCode() ^ BlockType.GetHashCode() ^ TransactionHeader.GetHashCode(); 
        }

        public IKey GetTransactionRegistryHashKey(ICryptoService cryptoService, IIdentityKeyProvider transactionHashKey)
        {
            byte[] transactionHeightBytes = BitConverter.GetBytes(BlockHeight);

            byte[] senderAndHeightBytes = new byte[Key.Length + transactionHeightBytes.Length];

            Array.Copy(Key.Value, senderAndHeightBytes, Key.Length);
            Array.Copy(transactionHeightBytes, 0, senderAndHeightBytes, Key.Length, transactionHeightBytes.Length);

            IKey key = transactionHashKey.GetKey(cryptoService.ComputeTransactionKey(senderAndHeightBytes));

            return key;
        }
    }
}