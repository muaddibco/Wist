using System;
using System.Buffers.Binary;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.ExtensionMethods;
using Wist.Core.Logging;
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
            _synchronizationContext = statesRepository.GetInstance<SynchronizationContext>();
        }

        public abstract PacketType PacketType { get; }

        /// <summary>
        /// Validates packet where packet being validated must be FULL packet but without DLE STX and global length specification
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public bool ValidatePacket(byte[] packet)
        {
            Span<byte> span = new Span<byte>(packet);

            ulong syncBlockHeight = BinaryPrimitives.ReadUInt64LittleEndian(span.Slice(2));

            if ((_synchronizationContext.LastBlockDescriptor?.BlockHeight.Equals(syncBlockHeight) ?? true) ||
                (_synchronizationContext.PrevBlockDescriptor?.BlockHeight.Equals(syncBlockHeight) ?? true))
            {
                bool res = ValidatePacket(span.Slice(10), syncBlockHeight);
                if (!res)
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

        protected abstract bool ValidatePacket(Span<byte> span, ulong syncBlockHeight);
    }
}
