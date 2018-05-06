namespace Wist.BlockLattice.Core.Enums
{
    public enum ChainType : ushort
    {
        Unknown = 0,
        AccountChain = 1,
        TransactionalChain = 2,
        Consensus = ushort.MaxValue
    }
}
