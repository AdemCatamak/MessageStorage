using FluentMigrator;
using MessageStorage.Configurations;

namespace MessageStorage.SqlServer.Migrations
{
    [Migration(10)]
    public class _0010_CreateIndex__JobTable_Status : Migration
    {
        private readonly MessageStorageRepositoryContextConfiguration _messageStorageRepositoryContextConfiguration;

        public _0010_CreateIndex__JobTable_Status(MessageStorageRepositoryContextConfiguration messageStorageRepositoryContextConfiguration)
        {
            _messageStorageRepositoryContextConfiguration = messageStorageRepositoryContextConfiguration;
        }

        public override void Up()
        {
            Create.Index("Jobs_JobStatus")
                  .OnTable("Jobs")
                  .InSchema(_messageStorageRepositoryContextConfiguration.Schema)
                  .OnColumn("JobStatus");
        }

        public override void Down()
        {
            Delete.Index("Jobs_JobStatus")
                  .OnTable("Jobs")
                  .InSchema(_messageStorageRepositoryContextConfiguration.Schema);
        }
    }
}