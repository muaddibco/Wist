using HashLib;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wist.BlockLattice.Core;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Cryptography;
using Wist.Core.HashCalculations;
using Wist.Core.Identity;
using Wist.Core.Logging;
using Wist.Core.States;
using Wist.Core.Synchronization;
using Wist.Node.Core.Registry;
using Wist.Tests.Core;
using Wist.Tests.Core.Fixtures;
using Xunit;

namespace Wist.Node.Core.Tests
{
    public class TransactionsRegistryMemPoolTests : IClassFixture<DependencyInjectionSupportFixture>
    {
        [Fact]
        public void MemPool_AddedXUniqueTransactions_AllContained()
        {
            SynchronizationDescriptor synchronizationDescriptor = new SynchronizationDescriptor(1, new byte[Globals.HASH_SIZE], DateTime.Now, DateTime.Now);
            IHash transactionKeyHash = HashFactory.Hash128.CreateMurmur3_128();
            ILogger logger = Substitute.For<ILogger>();
            ILoggerService loggerService = Substitute.For<ILoggerService>();
            IIdentityKeyProvider identityKeyProvider = new TransactionRegistryKeyProvider();
            IIdentityKeyProvidersRegistry identityKeyProvidersRegistry = Substitute.For<IIdentityKeyProvidersRegistry>();
            IHashCalculationsRepository hashCalculationRepository = Substitute.For<IHashCalculationsRepository>();
            ICryptoService cryptoService = Substitute.For<ICryptoService>();
            SynchronizationContext synchronizationContext = new SynchronizationContext(loggerService);
            synchronizationContext.UpdateLastSyncBlockDescriptor(synchronizationDescriptor);
            IStatesRepository statesRepository = Substitute.For<IStatesRepository>();

            logger.WhenForAnyArgs(l => l.Error(null)).DoNotCallBase();
            logger.WhenForAnyArgs(l => l.Warning(null)).DoNotCallBase();
            logger.WhenForAnyArgs(l => l.Info(null)).DoNotCallBase();
            loggerService.GetLogger(null).Returns(logger);

            identityKeyProvidersRegistry.GetInstance(null).ReturnsForAnyArgs(identityKeyProvider);

            statesRepository.GetInstance<ISynchronizationContext>().Returns(synchronizationContext);

            cryptoService.ComputeTransactionKey(null).ReturnsForAnyArgs(c => transactionKeyHash.ComputeBytes(c.Arg<byte[]>()).GetBytes());

            TransactionRegistryMemPool transactionRegistryMemPool = new TransactionRegistryMemPool(loggerService, identityKeyProvidersRegistry, cryptoService, statesRepository);

            byte[] privateKey = BinaryBuilder.GetRandomSeed();
            ulong expectedCount = 10;

            SortedList<ushort, TransactionRegisterBlock> expectedBlocks = new SortedList<ushort, TransactionRegisterBlock>();

            for (ulong i = 0; i < expectedCount; i++)
            {
                TransactionRegisterBlock transactionRegisterBlock = PacketsBuilder.GetTransactionRegisterBlock(
                    synchronizationContext.LastBlockDescriptor.BlockHeight, 1, null, i,
                    new TransactionHeader { ReferencedPacketType = PacketType.TransactionalChain, ReferencedBlockType = BlockTypes.Transaction_TransferFunds, ReferencedHeight = i, ReferencedBodyHash = new byte[Globals.POW_HASH_SIZE], ReferencedTargetHash = new byte[Globals.HASH_SIZE] },
                    privateKey);
                expectedBlocks.Add((ushort)i, transactionRegisterBlock);
                transactionRegistryMemPool.EnqueueTransactionRegisterBlock(transactionRegisterBlock);
            }

            SortedList<ushort, TransactionRegisterBlock> actualBlocks = transactionRegistryMemPool.DequeueBulk(-1);

            Assert.Equal(expectedCount, (ushort)actualBlocks.Count);
            for (ushort i = 0; i < (ushort)expectedCount; i++)
            {
                Assert.Equal(expectedBlocks[i].BlockHeight, actualBlocks[i].BlockHeight);
            }
        }

        [Fact]
        public void MemPool_AddedNonUniqueTransactions_NotAllContained()
        {
            SynchronizationDescriptor synchronizationDescriptor = new SynchronizationDescriptor(1, new byte[Globals.HASH_SIZE], DateTime.Now, DateTime.Now);
            IHash transactionKeyHash = HashFactory.Hash128.CreateMurmur3_128();
            ILogger logger = Substitute.For<ILogger>();
            ILoggerService loggerService = Substitute.For<ILoggerService>();
            IIdentityKeyProvider identityKeyProvider = new TransactionRegistryKeyProvider();
            IIdentityKeyProvidersRegistry identityKeyProvidersRegistry = Substitute.For<IIdentityKeyProvidersRegistry>();
            IHashCalculationsRepository hashCalculationRepository = Substitute.For<IHashCalculationsRepository>();
            ICryptoService cryptoService = Substitute.For<ICryptoService>();
            SynchronizationContext synchronizationContext = new SynchronizationContext(loggerService);
            synchronizationContext.UpdateLastSyncBlockDescriptor(synchronizationDescriptor);
            IStatesRepository statesRepository = Substitute.For<IStatesRepository>();

            logger.WhenForAnyArgs(l => l.Error(null)).DoNotCallBase();
            logger.WhenForAnyArgs(l => l.Warning(null)).DoNotCallBase();
            logger.WhenForAnyArgs(l => l.Info(null)).DoNotCallBase();
            loggerService.GetLogger(null).Returns(logger);

            identityKeyProvidersRegistry.GetInstance(null).ReturnsForAnyArgs(identityKeyProvider);

            statesRepository.GetInstance<ISynchronizationContext>().Returns(synchronizationContext);

            cryptoService.ComputeTransactionKey(null).ReturnsForAnyArgs(c => transactionKeyHash.ComputeBytes(c.Arg<byte[]>()).GetBytes());

            TransactionRegistryMemPool transactionRegistryMemPool = new TransactionRegistryMemPool(loggerService, identityKeyProvidersRegistry, cryptoService, statesRepository);

            byte[] privateKey = BinaryBuilder.GetRandomSeed();

            SortedList<ushort, TransactionRegisterBlock> expectedBlocks = new SortedList<ushort, TransactionRegisterBlock>();

            ulong[] heights = new ulong[] { 1, 2, 2, 5, 4, 3, 4, 3, 4, 5, 3};

            HashSet<ulong> addedHeights = new HashSet<ulong>();
            ushort order = 0;

            for (ulong i = 0; i < (ulong)heights.Length; i++)
            {
                TransactionRegisterBlock transactionRegisterBlock = PacketsBuilder.GetTransactionRegisterBlock(
                    synchronizationContext.LastBlockDescriptor.BlockHeight, 1, null, heights[i],
                    new TransactionHeader { ReferencedPacketType = PacketType.TransactionalChain, ReferencedBlockType = BlockTypes.Transaction_TransferFunds, ReferencedHeight = heights[i], ReferencedBodyHash = new byte[Globals.POW_HASH_SIZE], ReferencedTargetHash = new byte[Globals.HASH_SIZE] },
                    privateKey);

                if (!addedHeights.Contains(heights[i]))
                {
                    expectedBlocks.Add(order++, transactionRegisterBlock);
                    addedHeights.Add(heights[i]);
                }

                transactionRegistryMemPool.EnqueueTransactionRegisterBlock(transactionRegisterBlock);
            }

            SortedList<ushort, TransactionRegisterBlock> actualBlocks = transactionRegistryMemPool.DequeueBulk(-1);

            Assert.Equal(expectedBlocks.Count, actualBlocks.Count);
            for (ushort i = 0; i < (ushort)expectedBlocks.Count; i++)
            {
                Assert.Equal(expectedBlocks[i].BlockHeight, actualBlocks[i].BlockHeight);
            }
        }

        [Fact]
        public void MemPool_ContainsXItems_ConfidenceLevelOnAll()
        {
            SynchronizationDescriptor synchronizationDescriptor = new SynchronizationDescriptor(1, new byte[Globals.HASH_SIZE], DateTime.Now, DateTime.Now);
            IHash transactionKeyHash = HashFactory.Hash128.CreateMurmur3_128();
            ILogger logger = Substitute.For<ILogger>();
            ILoggerService loggerService = Substitute.For<ILoggerService>();
            IIdentityKeyProvider identityKeyProvider = new TransactionRegistryKeyProvider();
            IIdentityKeyProvidersRegistry identityKeyProvidersRegistry = Substitute.For<IIdentityKeyProvidersRegistry>();
            IHashCalculationsRepository hashCalculationRepository = Substitute.For<IHashCalculationsRepository>();
            ICryptoService cryptoService = Substitute.For<ICryptoService>();
            SynchronizationContext synchronizationContext = new SynchronizationContext(loggerService);
            synchronizationContext.UpdateLastSyncBlockDescriptor(synchronizationDescriptor);
            IStatesRepository statesRepository = Substitute.For<IStatesRepository>();

            logger.WhenForAnyArgs(l => l.Error(null)).DoNotCallBase();
            logger.WhenForAnyArgs(l => l.Warning(null)).DoNotCallBase();
            logger.WhenForAnyArgs(l => l.Info(null)).DoNotCallBase();
            loggerService.GetLogger(null).Returns(logger);

            identityKeyProvidersRegistry.GetInstance(null).ReturnsForAnyArgs(identityKeyProvider);

            statesRepository.GetInstance<ISynchronizationContext>().Returns(synchronizationContext);

            cryptoService.ComputeTransactionKey(null).ReturnsForAnyArgs(c => transactionKeyHash.ComputeBytes(c.Arg<byte[]>()).GetBytes());

            TransactionRegistryMemPool transactionRegistryMemPool = new TransactionRegistryMemPool(loggerService, identityKeyProvidersRegistry, cryptoService, statesRepository);

            byte[] privateKey = BinaryBuilder.GetRandomSeed();

            SortedList<ushort, TransactionRegisterBlock> expectedBlocks = new SortedList<ushort, TransactionRegisterBlock>();

            ulong[] heights = new ulong[] { 1, 2, 2, 5, 4, 3, 4, 3, 4, 5, 3 };

            HashSet<ulong> addedHeights = new HashSet<ulong>();
            ushort order = 0;

            for (ulong i = 0; i < (ulong)heights.Length; i++)
            {
                TransactionRegisterBlock transactionRegisterBlock = PacketsBuilder.GetTransactionRegisterBlock(
                    synchronizationContext.LastBlockDescriptor.BlockHeight, 1, null, heights[i],
                    new TransactionHeader { ReferencedPacketType = PacketType.TransactionalChain, ReferencedBlockType = BlockTypes.Transaction_TransferFunds, ReferencedHeight = heights[i], ReferencedBodyHash = new byte[Globals.POW_HASH_SIZE], ReferencedTargetHash = new byte[Globals.HASH_SIZE] },
                    privateKey);

                if (!addedHeights.Contains(heights[i]))
                {
                    expectedBlocks.Add(order++, transactionRegisterBlock);
                    addedHeights.Add(heights[i]);
                }

                transactionRegistryMemPool.EnqueueTransactionRegisterBlock(transactionRegisterBlock);
            }

            TransactionsShortBlock transactionsShortBlockAll = PacketsBuilder.GetTransactionsShortBlock(1, 1, null, 1, 1, expectedBlocks.Values, privateKey, cryptoService, identityKeyProvider);
            TransactionsShortBlock transactionsShortBlockOneLess = PacketsBuilder.GetTransactionsShortBlock(1, 1, null, 1, 1, expectedBlocks.Values.Skip(1), privateKey, cryptoService, identityKeyProvider);

            int confidenceLevelAll = transactionRegistryMemPool.GetConfidenceRate(transactionsShortBlockAll);
            int confidenceLevelOneLess = transactionRegistryMemPool.GetConfidenceRate(transactionsShortBlockOneLess);

            Assert.Equal(expectedBlocks.Count, confidenceLevelAll);
            Assert.Equal(expectedBlocks.Count - 1, confidenceLevelOneLess);
        }
    }
}
