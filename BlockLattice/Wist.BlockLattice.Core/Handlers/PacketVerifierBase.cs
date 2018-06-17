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
using Wist.Core.Logging;
using Wist.Core.ProofOfWork;
using Wist.Core.States;
using Wist.Core.Synchronization;

namespace Wist.BlockLattice.Core.Handlers
{
    public abstract class PacketVerifierBase : IPacketVerifier
    {
        protected readonly ILogger _log;
        protected readonly ISynchronizationContext _synchronizationContext;

        public PacketVerifierBase(IStatesRepository statesRepository, ILoggerService loggerService)
        {
            _log = loggerService.GetLogger(GetType().Name);
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
        }

        public abstract PacketType PacketType { get; }

        /// <summary>
        /// Validates packet where packet being validated must be FULL packet but without DLE STX and global length specification
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public bool ValidatePacket(byte[] packet)
        {
            using (MemoryStream ms = new MemoryStream(packet))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    SkipPacketType(br);
                    uint syncBlockHeight = br.ReadUInt32();

                    if (_synchronizationContext.LastBlockDescriptor.BlockHeight == syncBlockHeight || _synchronizationContext.LastBlockDescriptor.BlockHeight == syncBlockHeight)
                    {
                        bool res = ValidatePacket(br, syncBlockHeight);
                        if(!res)
                        {
                            _log.Error(packet.ToHexString());
                        }

                        return res;
                    }
                    else
                    {
                        _log.Error($"Synchronization block height is outdated: {packet.ToHexString()}");
                        return false;
                    }
                }
            }
        }

        private static void SkipPacketType(BinaryReader br)
        {
            br.ReadUInt16();
        }

        protected abstract bool ValidatePacket(BinaryReader br, uint syncBlockHeight);
    }
}
