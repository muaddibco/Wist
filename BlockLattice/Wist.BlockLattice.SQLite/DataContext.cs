using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.DataModel;

namespace Wist.BlockLattice.MySql
{
    public class DataContext : DbContext
    {
        public DbSet<AccountBlock> AccountBlocks { get; set; }

        public DbSet<AccountGenesis> AccountGenesises { get; set; }

        public DbSet<TransactionalBlock> TransactionalBlocks { get; set; }

        public DbSet<TransactionalGenesis> TransactionalGenesises { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=wallet.dat");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TransactionalGenesis>().HasIndex("OriginalHash");
            
            modelBuilder.Entity<TransactionalBlock>().HasIndex("BlockOrder");
        }
    }
}
