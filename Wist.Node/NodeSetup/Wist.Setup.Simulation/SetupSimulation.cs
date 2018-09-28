using Chaos.NaCl;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Wist.BlockLattice.SQLite.DataAccess;
using Wist.Core.Identity;

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
            if(resetDatabase)
            {
                DataAccessService.Instance.WipeAll();
            }

            DataAccessService.Instance.LoadAllIdentities();

            IEnumerable<IKey> keys = DataAccessService.Instance.GetAllAccountIdentities();

            int targetKeyNumber = 5 - keys.Count();

            List<IKey> keysToAdd = new List<IKey>();

            while (targetKeyNumber > 0)
            {
                byte[] seed = GetRandomSeed();
                byte[] keyBytes = Ed25519.PublicKeyFromSeed(seed);
                IKey key = _identityKeyProvider.GetKey(keyBytes);

                DataAccessService.Instance.AddIdentity(key);

                if (DataAccessService.Instance.AddSeed(key, seed))
                {
                    targetKeyNumber--;
                }
            }

            keys = DataAccessService.Instance.GetAllAccountIdentities();

            DataAccessService.Instance.EnsureChangesSaved();
        }

        private byte[] GetRandomSeed()
        {
            byte[] seed = new byte[32];
            RNGCryptoServiceProvider.Create().GetNonZeroBytes(seed);

            return seed;
        }
    }
}
