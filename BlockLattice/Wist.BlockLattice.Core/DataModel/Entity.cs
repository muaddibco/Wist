using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Models;

namespace Wist.BlockLattice.Core.DataModel
{
    public abstract class Entity
    {
        IKey Key { get; set; }
    }
}
