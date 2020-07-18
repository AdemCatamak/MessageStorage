using AccountWebApi.EntityFrameworkSection.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountWebApi.EntityFrameworkSection
{
    public class AccountDbContext : DbContext
    {
        public AccountDbContext(DbContextOptions<AccountDbContext> dbContextOptions)
            : base(dbContextOptions)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccountDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }

        public virtual DbSet<AccountModel> Accounts { get; set; }
    }
}