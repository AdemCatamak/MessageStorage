using FluentMigrator;
using MessageStorage.DataAccessLayer;

namespace Forgetty.SqlServer.Migrations
{
    [Migration(version: 3)]
    public class _0003_InsertInto__JobStatuses : Migration
    {
        private readonly RepositoryConfiguration _repositoryConfiguration;

        public _0003_InsertInto__JobStatuses(RepositoryConfiguration repositoryConfiguration)
        {
            _repositoryConfiguration = repositoryConfiguration;
        }

        public override void Up()
        {
            Insert.IntoTable("JobStatuses")
                  .InSchema(_repositoryConfiguration.Schema)
                  .Row(new {Id = 1, StatusName = "Queued"})
                  .Row(new {Id = 2, StatusName = "InProgress"})
                  .Row(new {Id = 3, StatusName = "Completed"})
                  .Row(new {Id = 4, StatusName = "Failed"});
        }

        public override void Down()
        {
            Delete.FromTable("JobStatuses")
                  .InSchema(_repositoryConfiguration.Schema)
                  .AllRows();
        }
    }
}