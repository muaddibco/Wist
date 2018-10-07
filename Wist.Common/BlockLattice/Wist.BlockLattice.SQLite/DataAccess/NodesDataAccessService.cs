using System.Collections.Generic;
using System.Linq;
using Wist.BlockLattice.DataModel;
using Wist.Core.Identity;
using System.Net;
using Wist.BlockLattice.Core.DataModel.Nodes;

namespace Wist.BlockLattice.SQLite.DataAccess
{
    public partial class DataAccessService
    {

        #region Nodes

        public bool AddNode(IKey key, NodeRole nodeRole, string ipAddressExpression = null)
        {
            if (IPAddress.TryParse(ipAddressExpression ?? "127.0.0.1", out IPAddress ipAddress))
            {
                return AddNode(key, nodeRole, ipAddress);
            }

            return false;
        }

        public bool AddNode(IKey key, NodeRole nodeRole, IPAddress ipAddress)
        {
            AccountIdentity accountIdentity = GetAccountIdentity(key);

            if (accountIdentity == null)
            {
                GetOrAddIdentity(key);

                accountIdentity = GetAccountIdentity(key);
            }

            if (accountIdentity != null)
            {
                lock (_sync)
                {
                    NodeRecord node = _dataContext.Nodes.FirstOrDefault(n => n.Identity == accountIdentity && n.NodeRole == (byte)nodeRole);

                    if (node == null)
                    {

                        node = new NodeRecord { Identity = accountIdentity, IPAddress = ipAddress.GetAddressBytes(), NodeRole = (byte)nodeRole };
                        _dataContext.Nodes.Add(node);
                        if (!_keyToNodeMap.ContainsKey(key))
                        {
                            _keyToNodeMap.Add(key, node);
                        }
                        return true;
                    }
                }
            }

            return false;
        }

        public bool UpdateNode(IKey key, string ipAddressExpression = null)
        {

            if (IPAddress.TryParse(ipAddressExpression ?? "127.0.0.1", out IPAddress ipAddress))
            {
                return UpdateNode(key, ipAddress);
            }

            return false;
        }

        public bool UpdateNode(IKey key, IPAddress ipAddress)
        {
            AccountIdentity accountIdentity = GetAccountIdentity(key);

            if (accountIdentity != null)
            {
                lock (_sync)
                {
                    IEnumerable<NodeRecord> nodes = _dataContext.Nodes.Where(n => n.Identity == accountIdentity);

                    foreach (var node in nodes)
                    {
                        node.IPAddress = ipAddress.GetAddressBytes();
                        _dataContext.Update(node);
                        _keyToNodeMap[key].IPAddress = ipAddress.GetAddressBytes();
                    }
                }
            }

            return false;
        }

        public void LoadAllKnownNodeIPs()
        {
            lock (_sync)
            {
                _keyToNodeMap = new Dictionary<IKey, NodeRecord>();
                foreach (var node in _dataContext.Nodes)
                {
                    IKey key = _identityKeyProvider.GetKey(node.Identity.PublicKey);
                    if(!_keyToNodeMap.ContainsKey(key))
                    {
                        _keyToNodeMap.Add(key, node);
                    }
                }
            }
        }

        public IPAddress GetNodeIpAddress(IKey key)
        {
            if (_keyToNodeMap.ContainsKey(key))
            {
                return new IPAddress(_keyToNodeMap[key].IPAddress);
            }

            return IPAddress.None;
        }

        public IEnumerable<NodeRecord> GetAllNodes()
        {
            lock (_sync)
            {
                List<NodeRecord> nodes = _dataContext.Nodes.ToList();

                return nodes;
            }
        }

        public NodeRecord GetNode(IKey key)
        {
            if (_keyToNodeMap.ContainsKey(key))
            {
                return _keyToNodeMap[key];
            }

            return null;
        }

        #endregion Nodes
    }
}
