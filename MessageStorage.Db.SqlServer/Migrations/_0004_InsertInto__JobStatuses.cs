using FluentMigrator;
using MessageStorage.Db.Configurations;

namespace MessageStorage.Db.SqlServer.Migrations
{
    [Migration(version: 4)]
    public class _0004_InsertInto__JobStatuses : Migration
    {
        private readonly DbRepositoryConfiguration _dbRepositoryConfiguration;

        public _0004_InsertInto__JobStatuses(DbRepositoryConfiguration dbRepositoryConfiguration)
        {
            _dbRepositoryConfiguration = dbRepositoryConfiguration;
        }

        public override void Up()
        {
            Insert.IntoTable("JobStatuses")
                  .InSchema(_dbRepositoryConfiguration.Schema)
                  .Row(new {StatusId = 1, StatusName = "Waiting"})
                  .Row(new {StatusId = 2, StatusName = "InProgress"})
                  .Row(new {StatusId = 3, StatusName = "Done"})
                  .Row(new {StatusId = 4, StatusName = "Failed"});
        }

        public override void Down()
        {
            Delete.FromTable("JobStatuses")
                  .InSchema(_dbRepositoryConfiguration.Schema)
                  .Row(new {StatusId = 1})
                  .Row(new {StatusId = 2})
                  .Row(new {StatusId = 3})
                  .Row(new {StatusId = 4});
        }
    }
}