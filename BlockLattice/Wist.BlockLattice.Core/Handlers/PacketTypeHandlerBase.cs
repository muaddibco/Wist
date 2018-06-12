using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.ExtensionMethods;
using Wist.Core.ProofOfWork;
using Wist.Core.Synchronization;

namespace Wist.BlockLattice.Core.Handlers
{
    public abstract class PacketTypeHandlerBase : IPacketTypeHandler
    {
        private readonly ILog _log;
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly IProofOfWorkCalculationFactory _proofOfWorkCalculationFactory;

        public PacketTypeHandlerBase(ISynchronizationContext synchronizationContext, IProofOfWorkCalculationFactory proofOfWorkCalculationFactory, IBlockParsersFactory[] blockParsersFactories)
        {
            _log = LogManager.GetLogger(GetType());
            BlockParsersFactory = blockParsersFactories.FirstOrDefault(f => f.ChainType == ChainType);
            _synchronizationContext = synchronizationContext;
            _proofOfWorkCalculationFactory = proofOfWorkCalculationFactory;
        }

        public abstract PacketType ChainType { get; }

        public IBlockParsersFactory BlockParsersFactory { get; }

        /// <summary>
        /// Validates packet where packet being validated must be FULL packet but without DLE STX and global length specification
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public PacketErrorMessage ValidatePacket(byte[] packet)
        {
            PacketErrorMessage packetErrorMessage;

            using (MemoryStream ms = new MemoryStream(packet))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    SkipPacketType(br);

                    PacketsErrors packetsError;
                    if (CheckSyncPOW(br, out packetsError))
                    {
                        packetErrorMessage = new PacketErrorMessage(ValidatePacket(br), packet);
                    }
                    else
                    {
                        packetErrorMessage = new PacketErrorMessage(packetsError, packet);
                    }
                }
            }

            return packetErrorMessage;
        }

        private bool CheckSyncPOW(BinaryReader br, out PacketsErrors packetsError)
        {
            ushort powTypeValue = br.ReadUInt16();
            POWType pOWType = (POWType)powTypeValue;

            IProofOfWorkCalculation proofOfWorkCalculation = _proofOfWorkCalculationFactory.Create(pOWType);

            uint syncBlockHeight = br.ReadUInt32();
            ulong nonce = br.ReadUInt64();
            byte[] hash = br.ReadBytes(proofOfWorkCalculation.HashSize);

            if(syncBlockHeight != _synchronizationContext.LastBlockDescriptor.BlockHeight && syncBlockHeight != _synchronizationContext.PrevBlockDescriptor.BlockHeight)
            {
                packetsError = PacketsErrors.SYNC_POW_OUTDATED;
                return false;
            }

            BigInteger bigInteger = new BigInteger(syncBlockHeight == _synchronizationContext.LastBlockDescriptor.BlockHeight ? _synchronizationContext.LastBlockDescriptor.Hash : _synchronizationContext.PrevBlockDescriptor.Hash);
            bigInteger += nonce;

            byte[] input = bigInteger.ToByteArray();
            byte[] computedHash = proofOfWorkCalculation.CalculateHash(input);

            if(!computedHash.EqualsX16(hash))
            {
                packetsError = PacketsErrors.SYNC_POW_MISMATCH;
                return false;
            }

            //TODO: Add difficulty check

            packetsError = PacketsErrors.NO_ERROR;
            return true;
        }

        private static void SkipPacketType(BinaryReader br)
        {
            br.ReadUInt16();
        }

        protected abstract PacketsErrors ValidatePacket(BinaryReader br);
    }
}
