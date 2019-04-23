using System.Collections.Generic;
using Wist.Blockchain.Core.DAL.Keys;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Identity;
using Wist.Core.Models;

namespace Wist.Blockchain.Core.Interfaces
{
    [ExtensionPoint]
    public interface IChainDataService : IDataService<PacketBase, IDataKey>
    {
        PacketType PacketType { get; }

        bool AreServiceActionsAllowed(IKey key);

        PacketBase GetLastBlock(IKey key);

        IEnumerable<PacketBase> GetAllByKey(IKey key);

        PacketBase GetBlockByOrder(IKey key, uint order);

        List<PacketBase> GetAllLastBlocksByType(ushort blockType);

        IEnumerable<T> GetAllLastBlocksByType<T>() where T : PacketBase;
    }
}
