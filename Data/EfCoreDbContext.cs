using Microsoft.EntityFrameworkCore;
using System;

namespace Data
{
    public class EfCoreDbContext : DbContext
    {
        public EfCoreDbContext() : base() { }

        public EfCoreDbContext(DbContextOptions options) : base(options) { }

        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
            {
                return;
            }

            optionsBuilder
                .UseSqlServer("Server=(LocalDb)\\MSSQLLocalDB;Database=DefaultEfCoreContext;Trusted_Connection=True;");
        }
    }
}
