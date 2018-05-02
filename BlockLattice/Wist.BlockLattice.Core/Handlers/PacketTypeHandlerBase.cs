using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.ExtensionMethods;

namespace Wist.BlockLattice.Core.Handlers
{
    public abstract class PacketTypeHandlerBase : IChainTypeValidationHandler
    {
        private readonly ILog _log;

        public PacketTypeHandlerBase()
        {
            _log = LogManager.GetLogger(GetType());
        }

        public abstract ChainType ChainType { get; }

        public PacketErrorMessage ProcessPacket(byte[] packet)
        {
            PacketErrorMessage packetErrorMessage;

            using (MemoryStream ms = new MemoryStream(packet))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    br.ReadUInt16();

                    packetErrorMessage = new PacketErrorMessage(ProcessPacket(br), packet);
                }
            }

            return packetErrorMessage;
        }

        protected abstract PacketsErrors ProcessPacket(BinaryReader br);
    }
}
