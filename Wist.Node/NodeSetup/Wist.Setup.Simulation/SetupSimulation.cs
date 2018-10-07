using Chaos.NaCl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Wist.BlockLattice.Core.DataModel.Nodes;
using Wist.BlockLattice.DataModel;
using Wist.BlockLattice.SQLite.DataAccess;
using Wist.Core.Cryptography;
using Wist.Core.Identity;
using Wist.Core.ExtensionMethods;

namespace Wist.Setup.Simulation
{
    public class SetupSimulation
    {
        private readonly Dictionary<string, string> _keys = new Dictionary<string, string> { };
        private readonly IIdentityKeyProvider _identityKeyProvider;

        public SetupSimulation(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry)
        {
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
        }

        public void Run(bool resetDatabase = false)
        {
            if (resetDatabase)
            {
                DataAccessService.Instance.WipeAll();
            }

            DataAccessService.Instance.LoadAllIdentities();

            SetupIdentities();

            SetupLocalAsNode();

            DataAccessService.Instance.EnsureChangesSaved();
        }

        private void SetupIdentities()
        {
            IEnumerable<IKey> keys = DataAccessService.Instance.GetAllAccountIdentities();

            int targetKeyNumber = 5 - keys.Count();

            List<IKey> keysToAdd = new List<IKey>();

            while (targetKeyNumber > 0)
            {
                byte[] seed = CryptoHelper.GetRandomSeed();
                byte[] keyBytes = Ed25519.PublicKeyFromSeed(seed);
                IKey key = _identityKeyProvider.GetKey(keyBytes);

                DataAccessService.Instance.GetOrAddIdentity(key);

                if (DataAccessService.Instance.AddSeed(key, seed))
                {
                    targetKeyNumber--;
                }
            }

            keys = DataAccessService.Instance.GetAllAccountIdentities();
        }

        private void SetupLocalAsNode()
        {
            IEnumerable<NodeRecord> nodes = DataAccessService.Instance.GetAllNodes();

            if(!nodes.Any(n => "127.0.0.1".Equals(new IPAddress(n.IPAddress).ToString())))
            {
                byte[] seed = CryptoHelper.GetRandomSeed();
                byte[] publicKey = Ed25519.PublicKeyFromSeed(seed);
                IKey key = _identityKeyProvider.GetKey(publicKey);

                DataAccessService.Instance.AddNode(key, NodeRole.TransactionsRegistrationLayer, IPAddress.Parse("127.0.0.1"));
                DataAccessService.Instance.AddNode(key, NodeRole.StorageLayer, IPAddress.Parse("127.0.0.1"));
                DataAccessService.Instance.AddNode(key, NodeRole.SynchronizationLayer, IPAddress.Parse("127.0.0.1"));

                Console.WriteLine($"Please copy carefully aside seed below for providing as input argument to Node executable:");
                Console.WriteLine(seed.ToHexString());
            }
        }
    }
}
