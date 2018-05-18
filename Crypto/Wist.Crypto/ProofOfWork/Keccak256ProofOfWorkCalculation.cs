using HashLib;
using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.ProofOfWork;

namespace Wist.Crypto.ProofOfWork
{
    [RegisterExtension(typeof(IProofOfWorkCalculation), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class Keccak256ProofOfWorkCalculation : IProofOfWorkCalculation
    {
        public POWType POWType => POWType.Keccak256;

        public ushort HashSize => 16;

        private readonly IHash _hash;

        public Keccak256ProofOfWorkCalculation()
        {
            _hash = HashFactory.Crypto.SHA3.CreateKeccak256();
        }

        public byte[] CalculateHash(byte[] input)
        {
            HashResult hash = _hash.ComputeBytes(input);
            return hash.GetBytes();
        }
    }
}
