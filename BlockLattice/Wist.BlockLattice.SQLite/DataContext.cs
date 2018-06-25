﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Wist.BlockLattice.DataModel;
using Wist.BlockLattice.SQLite.Configuration;

namespace Wist.BlockLattice.SQLite
{
    public class DataContext : DbContext
    {
        private readonly SQLiteConfiguration _configuration;
        private readonly ManualResetEventSlim _manualResetEventSlim = new ManualResetEventSlim(false);

        public DataContext(SQLiteConfiguration configuration)
        {
            _configuration = configuration;
        }

        public DbSet<AccountIdentity> AccountIdentities { get; set; }

        public DbSet<AccountBlock> AccountBlocks { get; set; }

        public DbSet<AccountGenesis> AccountGenesises { get; set; }

        public DbSet<TransactionalBlock> TransactionalBlocks { get; set; }

        public DbSet<TransactionalGenesis> TransactionalGenesises { get; set; }

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

            //modelBuilder.Entity<TransactionalGenesis>().HasIndex("OriginalHash");
            
            //modelBuilder.Entity<TransactionalBlock>().HasIndex("BlockOrder");
        }
    }
}
