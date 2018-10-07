using Microsoft.EntityFrameworkCore;
using System.Threading;
using Wist.BlockLattice.DataModel;
using Wist.BlockLattice.SQLite.Configuration;

namespace Wist.BlockLattice.SQLite
{
    public class DataContext : DbContext
    {
        private readonly ISQLiteConfiguration _configuration;
        private readonly ManualResetEventSlim _manualResetEventSlim = new ManualResetEventSlim(false);

        public DataContext(ISQLiteConfiguration configuration)
        {
            _configuration = configuration;
        }

        internal DbSet<AccountSeed> AccountSeeds { get; set; }

        public DbSet<AccountIdentity> AccountIdentities { get; set; }

        public DbSet<NodeRecord> Nodes { get; set; }

        public DbSet<TransactionalBlock> TransactionalBlocks { get; set; }

        public DbSet<TransactionalIdentity> TransactionalIdentities { get; set; }

        public DbSet<SynchronizationBlock> SynchronizationBlocks { get; set; }

        public DbSet<RegistryCombinedBlock> RegistryCombinedBlocks { get; set; }

        public DbSet<TransactionsRegistryBlock> TransactionsRegistryBlocks { get; set; }

        public void EnsureConfigurationCompleted()
        {
            _manualResetEventSlim.Wait();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_configuration.ConnectionString); //("Filename=wallet.dat");
            _manualResetEventSlim.Set();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AccountIdentity>().HasIndex(a => a.KeyHash);

            //modelBuilder.Entity<TransactionsRegistryBlock>().HasKey(p => new { p.TransactionsRegistryBlockId, p.ShardId });
            //modelBuilder.Entity<TransactionalGenesis>().HasIndex("OriginalHash");
            
            //modelBuilder.Entity<TransactionalBlock>().HasIndex("BlockOrder");
        }
    }
}
