using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.ProofOfWork;
using Wist.Core.Properties;

namespace Wist.Core.Exceptions
{

    [Serializable]
    public class ProofOfWorkAlgorithmNotSupportedException : Exception
    {
        public ProofOfWorkAlgorithmNotSupportedException() { }
        public ProofOfWorkAlgorithmNotSupportedException(POWType pOWType) : base(string.Format(Resources.ERR_PROOF_OF_WORK_ALGORITHM_NOT_SUPPORTED, pOWType)) { }
        public ProofOfWorkAlgorithmNotSupportedException(POWType pOWType, Exception inner) : base(string.Format(Resources.ERR_PROOF_OF_WORK_ALGORITHM_NOT_SUPPORTED, pOWType), inner) { }
        protected ProofOfWorkAlgorithmNotSupportedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
