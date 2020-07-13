using FluentMigrator;
using MessageStorage.Db.Configurations;

namespace MessageStorage.Db.SqlServer.Migrations
{
    [Migration(version: 8)]
    public class _0008_AlterTable__MessageTable_IdAsString : Migration
    {
        private readonly DbRepositoryConfiguration _dbRepositoryConfiguration;

        public _0008_AlterTable__MessageTable_IdAsString(DbRepositoryConfiguration dbRepositoryConfiguration)
        {
            _dbRepositoryConfiguration = dbRepositoryConfiguration;
        }

        public override void Up()
        {
            Execute.Sql(
                        CustomScriptBuilder.DeletePrimaryKeyScript(_dbRepositoryConfiguration.Schema, "Messages")
                       );
            
            Alter.Table("Messages")
                 .InSchema(_dbRepositoryConfiguration.Schema)
                 .AlterColumn("MessageId").AsString(size: 512);

            Create.PrimaryKey("PK_Messages_MessageId")
                  .OnTable("Messages")
                  .WithSchema(_dbRepositoryConfiguration.Schema)
                  .Column("MessageId");
        }

        public override void Down()
        {
            Delete.PrimaryKey("PK_Messages_MessageId")
                  .FromTable("Messages")
                  .InSchema(_dbRepositoryConfiguration.Schema);

            Alter.Table("Messages")
                 .InSchema(_dbRepositoryConfiguration.Schema)
                 .AlterColumn("MessageId").AsInt64();

            Create.PrimaryKey("PK_Messages_MessageId")
                  .OnTable("Messages")
                  .WithSchema(_dbRepositoryConfiguration.Schema)
                  .Column("MessageId");
        }
    }
}