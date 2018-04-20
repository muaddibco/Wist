namespace Wist.BlockLattice.Core.DataModel
{
    public enum TransactionalBlockType : ushort
    {
        Unknown = 0,
        AccountBlock = 1,
        AcceptFunds = 2,
        TransferFunds = 3,
        Confirm = 4
    }
}