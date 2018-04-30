using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Transactional;
using Wist.Core.Architecture;

namespace Wist.BlockLattice.Core.Interfaces
{
    [ServiceContract]
    public interface ILatticeDataService : IDisposable
    {
        /// <summary>
        /// Initializes service. This function must be called first.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Returns Transactional Blockchain having Genesis Block with Original Hash value as provided in argument
        /// </summary>
        /// <param name="originalHash">Original Hash value stored in Genesis Block of Transactional Blockchain to be returned</param>
        /// <returns>Transactional Blockchain having Genesis Block with Original Hash value as provided in argument</returns>
        TransactionalBlockchain GetTransactionalBlockchain(byte[] originalHash);

        /// <summary>
        /// Returns collection of blocks that are last in transactionsal blockchain. More than 1 block means that there are forks in blockchain.
        /// </summary>
        /// <param name="originalHash"></param>
        /// <returns></returns>
        HashSet<TransactionalBlockBase> GetLastTransactionalBlocks(byte[] originalHash);

        /// <summary>
        /// Creates Genesis Block with provided original hash value
        /// </summary>
        /// <param name="originalHash"></param>
        void CreateGenesisBlock(byte[] originalHash);
    }
}
