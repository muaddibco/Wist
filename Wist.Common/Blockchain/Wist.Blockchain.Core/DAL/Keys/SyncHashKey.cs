namespace Wist.Blockchain.Core.DAL.Keys
{
    public class SyncHashKey : IDataKey
    {
        public SyncHashKey(ulong syncBlockHeight, byte[] hash)
        {
            SyncBlockHeight = syncBlockHeight;
            Hash = hash;
        }

        public ulong SyncBlockHeight { get; set; }

        public byte[] Hash { get; set; }

    }
}
