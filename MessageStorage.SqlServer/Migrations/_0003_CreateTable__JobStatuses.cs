using FluentMigrator;
using MessageStorage.Configurations;

namespace MessageStorage.SqlServer.Migrations
{
    [Migration(version: 3)]
    public class _0003_CreateTable__JobStatuses : Migration
    {
        private readonly MessageStorageRepositoryContextConfiguration _messageStorageRepositoryContextConfiguration;

        public _0003_CreateTable__JobStatuses(MessageStorageRepositoryContextConfiguration messageStorageRepositoryContextConfiguration)
        {
            _messageStorageRepositoryContextConfiguration = messageStorageRepositoryContextConfiguration;
        }

        public override void Up()
        {
            Create.Table("JobStatuses")
                  .InSchema(_messageStorageRepositoryContextConfiguration.Schema)
                  .WithColumn("StatusId").AsInt32().PrimaryKey("PK_JobStatuses_StatusId")
                  .WithColumn("StatusName").AsString().NotNullable();
        }

        public override void Down()
        {
            Delete.Table("JobStatuses")
                  .InSchema(_messageStorageRepositoryContextConfiguration.Schema);
        }
    }
}