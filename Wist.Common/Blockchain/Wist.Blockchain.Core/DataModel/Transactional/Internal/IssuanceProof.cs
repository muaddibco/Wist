using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Cryptography;

namespace Wist.Blockchain.Core.DataModel.Transactional.Internal
{
    public class IssuanceProof
    {
        /// <summary>
        /// Surjection Proof 1 X 1 of Blinded Asset against issued raw AssetId
        /// </summary>
        public SurjectionProof SurjectionProof { get; set; }

        /// <summary>
        /// Masked Blinding Factor
        /// </summary>
        public byte[] Mask { get; set; }
    }
}
