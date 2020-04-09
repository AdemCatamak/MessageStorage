using Microsoft.EntityFrameworkCore;
using Samples.Db.WebApi.Data.Models;

namespace Samples.Db.WebApi.Data
{
    public class FooDbContext : DbContext
    {
        public FooDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
            base.OnModelCreating(modelBuilder);
        }

        public virtual DbSet<FooModel> Foo { get; set; }
    }
}