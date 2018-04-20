using Microsoft.EntityFrameworkCore;
using MySql.Data.EntityFrameworkCore.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.MySql.DataModel;

namespace Wist.BlockLattice.MySql
{
    public class DataContext : DbContext
    {
        public DbSet<TransactionalBlock> TransactionalBlocks { get; set; }

        public DbSet<TransactionalAccount> TransactionalAccounts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL("server=localhost;database=wistchain;user=lattice_user;password=Latt!cePassw0rd");
        }
    }
}
