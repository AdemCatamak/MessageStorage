using FluentMigrator;
using MessageStorage.DataAccessLayer;

namespace MessageStorage.MySql.Migrations
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
            Insert.IntoTable("job_statuses")
                  .InSchema(_repositoryConfiguration.Schema)
                  .Row(new {id = 1, status_name = "Queued"})
                  .Row(new {id = 2, status_name = "InProgress"})
                  .Row(new {id = 3, status_name = "Completed"})
                  .Row(new {id = 4, status_name = "Failed"});
        }

        public override void Down()
        {
            Delete.FromTable("job_statuses")
                  .InSchema(_repositoryConfiguration.Schema)
                  .AllRows();
        }
    }
}