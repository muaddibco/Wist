using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;

namespace Wist.BlockLattice.Core.Interfaces
{
    [ExtensionPoint]
    public interface IChainDataService
    {
        PacketType ChainType { get; }
        bool DoesChainExist(byte[] key);

        GenesisBlockBase GetGenesisBlock(byte[] key);

        BlockBase GetLastBlock(byte[] key);

        BlockBase GetBlockByOrder(byte[] key, uint order);

        BlockBase[] GetAllBlocks(byte[] key);

        void CreateGenesisBlock(GenesisBlockBase genesisBlock);

        void AddBlock(BlockBase block);

        List<BlockBase> GetAllLastBlocksByType(ushort blockType);
    }
}
