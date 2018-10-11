namespace Wist.BlockLattice.Core.Enums
{
    public enum PacketType : ushort
    {
        Transactional = 1,
        UtxoConfidential = 2,

        Storage = ushort.MaxValue - 2,
        Registry = ushort.MaxValue - 1,
        Synchronization = ushort.MaxValue
    }
}
