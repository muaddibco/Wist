using Wist.Core.HashCalculations;

namespace Wist.BlockLattice.Core
{
    public static class Globals
    {
        public const byte STX = 0x02;
        public const byte DLE = 0x10;
        public const byte PACKET_TYPE_LENGTH = 2;
        public const byte SYNC_BLOCK_HEIGHT_LENGTH = 8;
        public const byte NONCE_LENGTH = 4;
        public const byte TRANSACTION_KEY_HASH_SIZE = 16;
        public const byte POW_HASH_SIZE = 24;
        public const byte VERSION_LENGTH = 2;
        public const byte DEFAULT_HASH_SIZE = 32; // weigh using SHA3-256 with size of 32 byte
        public const byte NODE_PUBLIC_KEY_SIZE = 32;
        public const byte SIGNATURE_SIZE = 64;
        public const HashType TRANSACTION_KEY_HASH_TYPE = HashType.MurMur;
        public const HashType POW_TYPE = HashType.Tiger4;
        public const HashType DEFAULT_HASH = HashType.Keccak256;
    }
}
