using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.Core.Architecture;

namespace Wist.BlockLattice.Core.Interfaces
{
    [ServiceContract]
    public interface ILatticeDataService
    {
        /// <summary>
        /// Returns Transactional Blockchain having Genesis Block with Original Hash value as provided in argument
        /// </summary>
        /// <param name="originalHash">Original Hash value stored in Genesis Block of Transactional Blockchain to be returned</param>
        /// <returns>Transactional Blockchain having Genesis Block with Original Hash value as provided in argument</returns>
        TransactionalBlockchain GetTransactionalBlockchain(byte[] originalHash);
    }
}
