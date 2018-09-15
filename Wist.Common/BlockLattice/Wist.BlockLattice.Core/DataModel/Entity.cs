using Wist.Core.Identity;

namespace Wist.BlockLattice.Core.DataModel
{
    public abstract class Entity
    {
        public IKey Key { get; set; }
    }
}
