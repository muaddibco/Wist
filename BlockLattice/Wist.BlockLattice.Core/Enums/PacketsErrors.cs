namespace Wist.BlockLattice.Core.Enums
{
    public enum PacketsErrors : int
    {
        NO_ERROR = 0,
        LENGTH_IS_INVALID = 1,
        LENGTH_DOES_NOT_MATCH = 2,
        SIGNATURE_IS_INVALID = 3,
        HASHBACK_IS_INVALID = 4,
        INVALID_CONSENSUS_GROUP_PARTICIPANT = 5,
        SYNC_POW_MISMATCH = 6,
        SYNC_POW_OUTDATED = 7,
        SYNC_POW_DIFFICULTY_LOW = 8
    }
}