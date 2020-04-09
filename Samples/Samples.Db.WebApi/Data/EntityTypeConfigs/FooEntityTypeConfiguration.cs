using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Samples.Db.WebApi.Data.Models;

namespace Samples.Db.WebApi.Data.EntityTypeConfigs
{
    public class FooEntityTypeConfiguration : IEntityTypeConfiguration<FooModel>
    {
        public void Configure(EntityTypeBuilder<FooModel> builder)
        {
            builder.ToTable("Foo");
            builder.HasKey(model => model.Id);
        }
    }
}