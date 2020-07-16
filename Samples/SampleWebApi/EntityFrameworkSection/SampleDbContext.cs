using Microsoft.EntityFrameworkCore;
using SampleWebApi.EntityFrameworkSection.Models;

namespace SampleWebApi.EntityFrameworkSection
{
    public class SampleDbContext : DbContext
    {
        public SampleDbContext(DbContextOptions<SampleDbContext> dbContextOptions)
            : base(dbContextOptions)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SampleDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }

        public virtual DbSet<SampleModel> Samples { get; set; }
    }
}