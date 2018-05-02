namespace Wist.BlockLattice.Core.Enums
{
    public enum BlockType : ushort
    {
        Unknown = 0,
        Genesis = 1,
        Transaction_AccountBlock = 2,
        Transaction_AcceptFunds = 3,
        Transaction_TransferFunds = 4,
        Transaction_Confirm = 5
    }
}