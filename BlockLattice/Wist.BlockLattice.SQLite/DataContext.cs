using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.MySql.DataModel;

namespace Wist.BlockLattice.MySql
{
    public class DataContext : DbContext
    {
        public DbSet<TransactionalBlock> TransactionalBlocks { get; set; }

        public DbSet<TransactionalGenesis> TransactionalAccounts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=wallet.dat");
        }
    }
}
