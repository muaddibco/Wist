using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.BlockLattice.Core.PerformanceCounters;
using Wist.Core.ExtensionMethods;
using Wist.Core.Logging;
using Wist.Core.PerformanceCounters;

namespace Wist.BlockLattice.Core.Handlers
{
    public class PacketHandlingFlow
    {
        private readonly IEnumerable<ICoreVerifier> _coreVerifiers;
        private readonly IPacketVerifiersRepository _chainTypeValidationHandlersFactory;
        private readonly IBlockParsersRepositoriesRepository _blockParsersFactoriesRepository;
        private readonly IBlocksHandlersRegistry _blocksHandlersRegistry;
        private readonly EndToEndCountersService _endToEndCountersService;
        private readonly ILogger _log;

        private readonly TransformBlock<byte[], byte[]> _decodeBlock;
        private readonly TransformBlock<byte[], BlockBase> _parseBlock;
        private readonly ActionBlock<BlockBase> _processBlock;

        public PacketHandlingFlow(int iteration, ICoreVerifiersBulkFactory coreVerifiersBulkFactory, 
            IPacketVerifiersRepository packetTypeHandlersFactory, IBlockParsersRepositoriesRepository blockParsersFactoriesRepository, 
            IBlocksHandlersRegistry blocksProcessorFactory, IPerformanceCountersRepository performanceCountersRepository, ILoggerService loggerService)
        {
            _coreVerifiers = coreVerifiersBulkFactory.Create();
            _log = loggerService.GetLogger($"{nameof(PacketHandlingFlow)}#{iteration}");
            _blockParsersFactoriesRepository = blockParsersFactoriesRepository;
            _chainTypeValidationHandlersFactory = packetTypeHandlersFactory;
            _blocksHandlersRegistry = blocksProcessorFactory;

            _decodeBlock = new TransformBlock<byte[], byte[]>((Func<byte[], byte[]>)DecodeMessage, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1, BoundedCapacity = 1000000});
            _parseBlock = new TransformBlock<byte[], BlockBase>((Func<byte[], BlockBase>)ParseMessagePacket, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1, BoundedCapacity = 1000000 });
            _processBlock = new ActionBlock<BlockBase>((Action<BlockBase>)DispatchBlock, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1, BoundedCapacity = 1000000 });
            _endToEndCountersService = performanceCountersRepository.GetInstance<EndToEndCountersService>();

            _decodeBlock.LinkTo(_parseBlock);
            _parseBlock.LinkTo(_processBlock, ValidateBlock);
        }

        public async Task PostMessage(byte[] messagePacket)
        {
            await _decodeBlock.SendAsync(messagePacket);
        }

        private byte[] DecodeMessage(byte[] messagePacket)
        {
            MemoryStream memoryStream = new MemoryStream();

            bool dleDetected = false;

            foreach (byte b in messagePacket)
            {
                if (b != Globals.DLE)
                {
                    if (dleDetected)
                    {
                        dleDetected = false;
                        memoryStream.WriteByte((byte)(b - Globals.DLE));
                    }
                    else
                    {
                        memoryStream.WriteByte(b);
                    }
                }
                else
                {
                    dleDetected = true;
                }
            }

            _endToEndCountersService.DecodingThroughput.Increment();

            return memoryStream.ToArray();
        }

        private BlockBase ParseMessagePacket(byte[] messagePacket)
        {
            PacketType packetType = (PacketType)BitConverter.ToUInt16(messagePacket, 0);
            BlockBase blockBase = null;
            IBlockParser blockParser = null;
            IBlockParsersRepository blockParsersFactory = null;
            try
            {
                //TODO: weigh assumption that all messages are sync based (have reference to latest Sync Block)
                ushort blockType = BitConverter.ToUInt16(messagePacket, Globals.PACKET_TYPE_LENGTH + Globals.SYNC_BLOCK_HEIGHT_LENGTH + Globals.NONCE_LENGTH + Globals.POW_HASH_SIZE + Globals.VERSION_LENGTH);

                blockParsersFactory = _blockParsersFactoriesRepository.GetBlockParsersRepository(packetType);

                if (blockParsersFactory != null)
                {
                    blockParser = blockParsersFactory.GetInstance(blockType);

                    if (blockParser != null)
                    {
                        blockBase = blockParser.Parse(messagePacket);
                    }
                    else
                    {
                        _log.Error($"Block parser of packet type {packetType} and block type {blockType} not found! Message: {messagePacket.ToHexString()}");
                    }
                }
                else
                {
                    _log.Error($"Block parser factory of packet type {packetType} not found! Message: {messagePacket.ToHexString()}");
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to parse message {messagePacket.ToHexString()}", ex);
            }

            _endToEndCountersService.ParsingThroughput.Increment();

            return blockBase;
        }

        private bool ValidateBlock(BlockBase blockBase)
        {
            if (blockBase == null)
            {
                return false;
            }

            foreach (ICoreVerifier coreVerifier in _coreVerifiers)
            {
                if(!coreVerifier.VerifyBlock(blockBase))
                {
                    return false;
                }
            }

            IPacketVerifier packetVerifier = _chainTypeValidationHandlersFactory.GetInstance(blockBase.PacketType);

            bool res = packetVerifier?.ValidatePacket(blockBase) ?? true;

            _endToEndCountersService.CoreValidationThroughput.Increment();

            return res;
        }

        private void DispatchBlock(BlockBase block)
        {
            if (block != null)
            {
                IEnumerable<IBlocksHandler> blocksProcessors = _blocksHandlersRegistry.GetBulkInstances(block.PacketType);

                //TODO: weigh to check whether number of processors is greater than 1 before parallelizing for sake of performance
                blocksProcessors.AsParallel().ForAll(p => p.ProcessBlock(block));

                _endToEndCountersService.DispatchThroughput.Increment();
            }
        }
    }
}
