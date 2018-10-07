using System.Collections.Generic;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Identity;

namespace Wist.BlockLattice.Core.Interfaces
{
    [ExtensionPoint]
    public interface IChainDataService : IDataService<BlockBase, long>
    {
        PacketType ChainType { get; }

        bool DoesChainExist(IKey key);

        BlockBase GetLastBlock(IKey key);

        IEnumerable<BlockBase> GetAllByKey(IKey key);

        BlockBase GetBlockByOrder(IKey key, uint order);

        List<BlockBase> GetAllLastBlocksByType(ushort blockType);

        IEnumerable<T> GetAllLastBlocksByType<T>() where T : BlockBase;
    }
}
