using Wist.Core.Identity;
using Wist.Core.Models;

namespace Wist.Node.Core.Common
{
    public class SynchronizationGroupParticipant : Entity
    {
        public IKey Key { get; set; }

        public uint Weight { get; set; }
    }
}
