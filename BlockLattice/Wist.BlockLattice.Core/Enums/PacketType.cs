namespace Wist.BlockLattice.Core.Enums
{
    public enum PacketType : ushort
    {
        Unknown = 0,
        AccountChain = 1,
        TransactionalChain = 2,
        Synchronization = ushort.MaxValue - 1,
        Consensus = ushort.MaxValue
    }
}
