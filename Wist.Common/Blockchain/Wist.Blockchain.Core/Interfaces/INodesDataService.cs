using Wist.Blockchain.Core.DAL.Keys;
using Wist.Blockchain.Core.DataModel.Nodes;
using Wist.Core.Architecture;
using Wist.Core.Identity;

namespace Wist.Blockchain.Core.Interfaces
{
    [ServiceContract]
    public interface INodesDataService : IDataService<Node, UniqueKey>
    {
    }
}
