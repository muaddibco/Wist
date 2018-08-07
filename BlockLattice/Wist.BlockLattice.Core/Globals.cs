using Wist.Core.ProofOfWork;

namespace Wist.BlockLattice.Core
{
    public static class Globals
    {
        public const byte STX = 0x02;
        public const byte DLE = 0x10;
        public const byte PACKET_TYPE_LENGTH = 2;
        public const byte SYNC_BLOCK_HEIGHT_LENGTH = 8;
        public const byte NONCE_LENGTH = 4;
        public const byte POW_HASH_SIZE = 24;
        public const byte VERSION_LENGTH = 4;
        public const byte HASH_SIZE = 64;
        public const byte NODE_PUBLIC_KEY_SIZE = 32;
        public const byte SIGNATURE_SIZE = 64;
        public const POWType POW_TYPE = POWType.Tiger4;
    }
}
