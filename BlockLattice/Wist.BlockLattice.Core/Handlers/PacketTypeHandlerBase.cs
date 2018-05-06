using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.ExtensionMethods;

namespace Wist.BlockLattice.Core.Handlers
{
    public abstract class PacketTypeHandlerBase : IChainTypeHandler
    {
        private readonly ILog _log;

        public PacketTypeHandlerBase(IBlockParsersFactory[] blockParsersFactories)
        {
            _log = LogManager.GetLogger(GetType());
            BlockParsersFactory = blockParsersFactories.FirstOrDefault(f => f.ChainType == ChainType);
        }

        public abstract ChainType ChainType { get; }

        public IBlockParsersFactory BlockParsersFactory { get; }

        public PacketErrorMessage ValidatePacket(byte[] packet)
        {
            PacketErrorMessage packetErrorMessage;

            using (MemoryStream ms = new MemoryStream(packet))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    br.ReadUInt16();

                    packetErrorMessage = new PacketErrorMessage(ValidatePacket(br), packet);
                }
            }

            return packetErrorMessage;
        }

        protected abstract PacketsErrors ValidatePacket(BinaryReader br);
    }
}
