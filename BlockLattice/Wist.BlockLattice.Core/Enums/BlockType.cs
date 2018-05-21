namespace Wist.BlockLattice.Core.Enums
{
    public static class BlockTypes
    {
        public const ushort Unknown = 0;
        public const ushort Genesis = 1;

        public const ushort Transaction_Genesis = 2;
        public const ushort Transaction_AcceptFunds = 3;
        public const ushort Transaction_TransferFunds = 4;
        public const ushort Transaction_Confirm = 5;
        public const ushort Transaction_Dpos = 6;

        public const ushort Consensus_GenericConsensus = ushort.MaxValue;
        public const ushort Synchronization_TimeSyncBlock = 1;
        public const ushort Synchronization_RetransmissionBlock = 2;
        public const ushort Synchronization_ReadyToParticipateBlock = 3;
        public const ushort Synchronization_MedianApproval = 4;
        public const ushort Synchronization_ConfirmedBlock = ushort.MaxValue;
    }
}