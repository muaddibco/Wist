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
            IPAddress ipAddress;
            if (IPAddress.TryParse(ipAddressExpression ?? "127.0.0.1", out ipAddress))
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
                AddIdentity(key);

                accountIdentity = GetAccountIdentity(key);
            }

            if (accountIdentity != null)
            {
                lock (_sync)
                {
                    DataModel.Node node = _dataContext.Nodes.FirstOrDefault(n => n.Identity == accountIdentity);

                    if (node == null)
                    {

                        node = new DataModel.Node { Identity = accountIdentity, IPAddress = ipAddress.GetAddressBytes() };
                        _dataContext.Nodes.Add(node);
                        _keyToNodeMap.Add(key, node);
                        return true;
                    }
                }
            }

            return false;
        }

        public bool UpdateNode(IKey key, string ipAddressExpression = null)
        {
            IPAddress ipAddress;

            if (IPAddress.TryParse(ipAddressExpression ?? "127.0.0.1", out ipAddress))
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
                    DataModel.Node node = _dataContext.Nodes.FirstOrDefault(n => n.Identity == accountIdentity);

                    if (node != null)
                    {
                        node.IPAddress = ipAddress.GetAddressBytes();
                        _dataContext.Update<DataModel.Node>(node);
                        _keyToNodeMap[key].IPAddress = ipAddress.GetAddressBytes();
                        return true;
                    }
                }
            }

            return false;
        }

        public void LoadAllKnownNodeIPs()
        {
            lock (_sync)
            {
                _keyToNodeMap = _dataContext.Nodes.ToDictionary(i => _identityKeyProvider.GetKey(i.Identity.PublicKey), i => i);
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

        public IEnumerable<DataModel.Node> GetAllNodes()
        {
            return _keyToNodeMap.Values;
        }

        public DataModel.Node GetNode(IKey key)
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
