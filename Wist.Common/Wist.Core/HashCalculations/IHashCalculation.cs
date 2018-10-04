using System;
using Wist.Core.Architecture;

namespace Wist.Core.HashCalculations
{
    [ExtensionPoint]
    public interface IHashCalculation
    {
        HashType HashType { get; }

        int HashSize { get; }

        byte[] CalculateHash(byte[] input);

        byte[] CalculateHash(Memory<byte> input);
    }
}
