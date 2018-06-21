using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Models;

namespace Wist.BlockLattice.Core.Interfaces
{
    [ExtensionPoint]
    public interface IChainDataService : IDataService<BlockBase>
    {
        PacketType ChainType { get; }

        bool DoesChainExist(IKey key);

        GenesisBlockBase GetGenesisBlock(IKey key);

        BlockBase GetLastBlock(IKey key);

        IEnumerable<BlockBase> GetAllByKey(IKey key);

        BlockBase GetBlockByOrder(IKey key, uint order);

        void CreateGenesisBlock(GenesisBlockBase genesisBlock);

        List<BlockBase> GetAllLastBlocksByType(ushort blockType);
    }
}
