using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Identity;
using Wist.Core.Translators;
using NodeModel = Wist.BlockLattice.Core.DataModel.Nodes.Node;

namespace Wist.BlockLattice.SQLite.Mappers
{
    [RegisterExtension(typeof(ITranslator), Lifetime = LifetimeManagement.Singleton)]
    public class NodesMapper : TranslatorBase<DataModel.NodeRecord, NodeModel>
    {
        private readonly IIdentityKeyProvidersRegistry _identityKeyProvidersRegistry;

        public NodesMapper(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry)
        {
            _identityKeyProvidersRegistry = identityKeyProvidersRegistry;
        }

        public override NodeModel Translate(DataModel.NodeRecord nodeDataModel)
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
