using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Core.ProofOfWork
{
    public enum POWType : ushort
    {
        None = 0,
        Keccak256,
        MurMur,
        Tiger4
    }
}
