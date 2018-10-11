namespace Wist.BlockLattice.Core.Enums
{
    public static class BlockTypes
    {
        public const ushort Unknown = 0;
        public const ushort Registry_Register = 1;
        public const ushort Registry_FullBlock = 2;
        public const ushort Registry_ShortBlock = 3;
        public const ushort Registry_ConfidenceBlock = 4;
        public const ushort Registry_ConfirmationBlock = 5;

        public const ushort Transaction_TransferFunds = 1;
        public const ushort Transaction_AcceptFunds = 2;

        public const ushort UtxoConfidential_FundsTransfer = 1;
        public const ushort UtxoConfidential_NonQuantitativeAssetTransfer = 2;
        public const ushort UtxoConfidential_QuantitativeAssetTransfer = 3;

        public const ushort Consensus_GenericConsensus = ushort.MaxValue;

        public const ushort Synchronization_TimeSyncProducingBlock = 1;
        public const ushort Synchronization_RetransmissionBlock = 2;
        public const ushort Synchronization_ReadyToParticipateBlock = 3;
        public const ushort Synchronization_MedianApproval = 4;
        public const ushort Synchronization_RegistryCombinationBlock = 5;
        public const ushort Synchronization_ConfirmedBlock = ushort.MaxValue;

        public const ushort Storage_TransactionFull = 1;
    }
}