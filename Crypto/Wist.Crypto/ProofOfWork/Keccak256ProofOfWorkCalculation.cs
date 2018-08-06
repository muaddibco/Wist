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

        public int HashSize => _hash.HashSize;

        private readonly IHash _hash;

        public Keccak256ProofOfWorkCalculation()
        {
            _hash = HashFactory.Crypto.SHA3.CreateKeccak256();
        }

        public byte[] CalculateHash(byte[] input)
        {
            HashResult hashRes = _hash.ComputeBytes(input);
            return hashRes.GetBytes();
        }
    }
}
