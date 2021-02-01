using FluentMigrator;
using MessageStorage.Configurations;

namespace MessageStorage.SqlServer.Migrations
{
    [Migration(version: 4)]
    public class _0004_InsertInto__JobStatuses : Migration
    {
        private readonly MessageStorageRepositoryContextConfiguration _messageStorageRepositoryContextConfiguration;

        public _0004_InsertInto__JobStatuses(MessageStorageRepositoryContextConfiguration messageStorageRepositoryContextConfiguration)
        {
            _messageStorageRepositoryContextConfiguration = messageStorageRepositoryContextConfiguration;
        }

        public override void Up()
        {
            Insert.IntoTable("JobStatuses")
                  .InSchema(_messageStorageRepositoryContextConfiguration.Schema)
                  .Row(new {StatusId = 1, StatusName = "Waiting"})
                  .Row(new {StatusId = 2, StatusName = "InProgress"})
                  .Row(new {StatusId = 3, StatusName = "Done"})
                  .Row(new {StatusId = 4, StatusName = "Failed"});
        }

        public override void Down()
        {
            Delete.FromTable("JobStatuses")
                  .InSchema(_messageStorageRepositoryContextConfiguration.Schema)
                  .Row(new {StatusId = 1})
                  .Row(new {StatusId = 2})
                  .Row(new {StatusId = 3})
                  .Row(new {StatusId = 4});
        }
    }
}