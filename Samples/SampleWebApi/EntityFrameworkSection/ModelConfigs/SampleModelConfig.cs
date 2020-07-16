using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SampleWebApi.EntityFrameworkSection.Models;

namespace SampleWebApi.EntityFrameworkSection.ModelConfigs
{
    public class SampleModelConfig : IEntityTypeConfiguration<SampleModel>
    {
        public void Configure(EntityTypeBuilder<SampleModel> builder)
        {
            builder.ToTable("Samples");
            builder.HasKey(b => b.Id);
        }
    }
}