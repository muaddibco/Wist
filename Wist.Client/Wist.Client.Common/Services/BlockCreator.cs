using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.DataModel.Transactional;
using Wist.Client.Common.Exceptions;
using Wist.Client.Common.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Client.Common.Services
{
    /// <classDetails>   
    ///*****************************************************************
    ///  Machine Name : AMI-PC
    /// 
    ///  Author       : Ami
    ///       
    ///  Date         : 10/9/2018 10:54:47 PM      
    /// *****************************************************************/
    /// </classDetails>
    /// <summary>
    /// TODO: consider static class!!!!!!!!
    /// </summary>
    [RegisterDefaultImplementation(typeof(IBlockCreator), Lifetime = LifetimeManagement.Singleton)]
    public class BlockCreator : IBlockCreator
    {
        //============================================================================
        //                                 MEMBERS
        //============================================================================


        //============================================================================
        //                                  C'TOR
        //============================================================================

        public BlockCreator()
        {

        }

        /// <summary>
        /// TODO: remove this switch case and create smart system to instanciate objects 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public BlockBase GetInstance(ushort key)
        {
            switch (key)
            {
                case 1:
                    return new TransferFundsBlock();
                case 2:
                    return new RegistryRegisterBlock();
                default:
                    throw new UnknownTypeException();
            }
        }

        //============================================================================
        //                                FUNCTIONS
        //============================================================================

        #region ============ PUBLIC FUNCTIONS =============  


        #endregion

        #region ============ PRIVATE FUNCTIONS ============ 


        #endregion

    }
}
