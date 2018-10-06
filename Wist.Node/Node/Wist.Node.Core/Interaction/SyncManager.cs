using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Wist.BlockLattice.Core;
using Wist.Core.Synchronization;
using Wist.Proto.Model;

namespace Wist.Node.Core.Interaction
{
    public class SyncManagerImpl : SyncManager.SyncManagerBase
    {
        private readonly ISynchronizationContext _synchronizationContext;

        public SyncManagerImpl(ISynchronizationContext synchronizationContext)
        {
            _synchronizationContext = synchronizationContext;
        }

        public override Task<LastSyncBlock> GetLastSyncBlock(LastSyncBlock request, ServerCallContext context)
        {
            return Task.FromResult(new LastSyncBlock
            {
                Height = _synchronizationContext?.LastBlockDescriptor.BlockHeight ?? 0,
                Hash = ByteString.CopyFrom(_synchronizationContext?.LastBlockDescriptor.Hash ?? new byte[Globals.DEFAULT_HASH_SIZE])
            });
        }
    }
}
