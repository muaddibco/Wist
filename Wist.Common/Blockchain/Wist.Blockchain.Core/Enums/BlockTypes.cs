namespace Wist.Blockchain.Core.Enums
{
    public static class BlockTypes
    {
		/// <summary>
		/// This flag means that transaction is intended for transferring from State based account to UTXO one
		/// </summary>
		public const ushort TransitionalFlag = 0xF000;

        public const ushort Unknown = 0;
        public const ushort Registry_Register = 1;
        public const ushort Registry_FullBlock = 2;
        public const ushort Registry_ShortBlock = 3;
        public const ushort Registry_ConfidenceBlock = 4;
        public const ushort Registry_ConfirmationBlock = 5;
        public const ushort Registry_RegisterUtxoConfidential = 6;
		public const ushort Registry_RegisterTransfer = 7;

		public const ushort Transaction_TransferFunds = 1;
        public const ushort Transaction_AcceptFunds = 2;
        public const ushort Transaction_IssueGroupedAssets = 3;
		public const ushort Transaction_TransferGroupedAssets = 4;
		public const ushort Transaction_TransferGroupedAssetsToUtxo = 4 + TransitionalFlag;
        public const ushort Transaction_AcceptAssets = 5;
        public const ushort Transaction_IssueAssociatedAsset = 6;
        public const ushort Transaction_IssueAsset = 7;
		public const ushort Transaction_TransferAsset = 8;
		public const ushort Transaction_TransferAssetToUtxo = 8 + TransitionalFlag;
        public const ushort Transaction_BlindAsset = 9;
        public const ushort Transaction_RetransferAssetToUtxo = 10 + TransitionalFlag;
        public const ushort Transaction_IssueBlindedAsset = 11;
        public const ushort Transaction_IssueAssociatedBlindedAsset = 12;

		public const ushort UtxoConfidential_FundsTransfer = 1;
        public const ushort UtxoConfidential_NonQuantitativeAssetTransfer = 2;
        public const ushort UtxoConfidential_QuantitativeAssetTransfer = 3;
        public const ushort UtxoConfidential_NonQuantitativeTransitionAssetTransfer = 4;
        public const ushort UtxoConfidential_RequestAssociatedAssetCreation = 5;
        public const ushort UtxoConfidential_TransitionOnboardingDisclosingProofs = 6;
        public const ushort UtxoConfidential_TransitionAuthenticationProofs = 7;

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