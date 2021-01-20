using FluentMigrator;
using MessageStorage.Configurations;

namespace MessageStorage.SqlServer.Migrations
{
    [Migration(version: 9)]
    public class _0009_AlterTable__JobTable_IdAsString : Migration
    {
        private readonly MessageStorageRepositoryContextConfiguration _messageStorageRepositoryContextConfiguration;

        public _0009_AlterTable__JobTable_IdAsString(MessageStorageRepositoryContextConfiguration messageStorageRepositoryContextConfiguration)
        {
            _messageStorageRepositoryContextConfiguration = messageStorageRepositoryContextConfiguration;
        }

        public override void Up()
        {
            Execute.Sql(CustomScriptBuilder.DeletePrimaryKeyScript(_messageStorageRepositoryContextConfiguration.Schema, "Jobs"));

            Alter.Table("Jobs")
                 .InSchema(_messageStorageRepositoryContextConfiguration.Schema)
                 .AlterColumn("JobId").AsString(size: 512);

            Create.PrimaryKey("PK_Jobs_JobId")
                  .OnTable("Jobs")
                  .WithSchema(_messageStorageRepositoryContextConfiguration.Schema)
                  .Column("JobId");
        }

        public override void Down()
        {
            Delete.PrimaryKey("PK_Jobs_JobId")
                  .FromTable("Jobs")
                  .InSchema(_messageStorageRepositoryContextConfiguration.Schema);

            Alter.Table("Jobs")
                 .InSchema(_messageStorageRepositoryContextConfiguration.Schema)
                 .AlterColumn("JobId").AsInt64();

            Create.PrimaryKey("PK_Jobs_JobId")
                  .OnTable("Jobs")
                  .WithSchema(_messageStorageRepositoryContextConfiguration.Schema)
                  .Column("JobId");
        }
    }
}