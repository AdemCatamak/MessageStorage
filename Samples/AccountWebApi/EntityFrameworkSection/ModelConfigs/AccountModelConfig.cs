using AccountWebApi.EntityFrameworkSection.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccountWebApi.EntityFrameworkSection.ModelConfigs
{
    public class AccountModelConfig : IEntityTypeConfiguration<AccountModel>
    {
        public void Configure(EntityTypeBuilder<AccountModel> builder)
        {
            builder.ToTable("Accounts");
            builder.HasKey(b => b.Id);
        }
    }
}