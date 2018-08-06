using Wist.Core.ProofOfWork;

namespace Wist.BlockLattice.Core
{
    public static class Globals
    {
        public static byte HASH_SIZE = 64;
        public static byte NODE_PUBLIC_KEY_SIZE = 32;
        public static byte SIGNATURE_SIZE = 64;
        public static byte POW_HASH_SIZE = 8;
        public static POWType POW_TYPE = POWType.MurMur;
    }
}
