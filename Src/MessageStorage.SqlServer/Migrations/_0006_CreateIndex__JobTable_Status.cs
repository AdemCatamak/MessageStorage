using FluentMigrator;
using MessageStorage.DataAccessLayer;

namespace MessageStorage.SqlServer.Migrations
{
    [Migration(6)]
    public class _0006_CreateIndex__JobTable_Status : Migration
    {
        private readonly RepositoryConfiguration _repositoryConfiguration;

        public _0006_CreateIndex__JobTable_Status(RepositoryConfiguration repositoryConfiguration)
        {
            _repositoryConfiguration = repositoryConfiguration;
        }

        public override void Up()
        {
            Create.Index("IX_Jobs_JobStatus")
                  .OnTable("Jobs")
                  .InSchema(_repositoryConfiguration.Schema)
                  .OnColumn("JobStatus");
        }

        public override void Down()
        {
            Delete.Index("IX_Jobs_JobStatus")
                  .OnTable("Jobs")
                  .InSchema(_repositoryConfiguration.Schema);
        }
    }
}