using Wist.Core.Identity;
using Wist.Core.Mappers;
using NodeModel = Wist.BlockLattice.Core.DataModel.Nodes.Node;

namespace Wist.BlockLattice.SQLite.Mappers
{
    public class NodesMapper : MapperBase<DataModel.Node, NodeModel>
    {
        private readonly IIdentityKeyProvidersRegistry _identityKeyProvidersRegistry;

        public NodesMapper(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry)
        {
            _identityKeyProvidersRegistry = identityKeyProvidersRegistry;
        }

        public override NodeModel Convert(DataModel.Node nodeDataModel)
        {
            NodeModel node = new NodeModel
            {
                Key = _identityKeyProvidersRegistry.GetInstance().GetKey(nodeDataModel.Identity.PublicKey),
                IPAddress = new System.Net.IPAddress(nodeDataModel.IPAddress)
            };

            return node;
        }
    }
}
