using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Enums;
using Wist.Core.Architecture;

namespace Wist.BlockLattice.Core.Interfaces
{
    public interface IChainDataService<TGenesisBlock, TBlockBase> : IChainDataService
        where TGenesisBlock : GenesisBlockBase 
        where TBlockBase : BlockBase
    {
        TGenesisBlock GetGenesisBlock(byte[] key);

        TBlockBase GetLastBlock(byte[] key);

        TBlockBase GetBlockByOrder(byte[] key, uint order);

        TBlockBase[] GetAllBlocks(byte[] key);

        void CreateGenesisBlock(TGenesisBlock genesisBlock);

        void AddBlock(TBlockBase block);
    }

    [ExtensionPoint]
    public interface IChainDataService
    {
        ChainType ChainType { get; }
    }
}
