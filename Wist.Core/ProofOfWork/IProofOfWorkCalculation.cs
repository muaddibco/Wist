using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;

namespace Wist.Core.ProofOfWork
{
    [ExtensionPoint]
    public interface IProofOfWorkCalculation
    {
        POWType POWType { get; }

        ushort HashSize { get; }

        byte[] CalculateHash(byte[] input);
    }
}
