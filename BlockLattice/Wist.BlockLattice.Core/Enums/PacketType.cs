namespace Wist.BlockLattice.Core.Enums
{
    public enum PacketType : ushort
    {
        Registry = 0,
        AccountChain = 1,
        TransactionalChain = 2,
        Storage = 3,
        Synchronization = ushort.MaxValue
    }
}
