using FluentMigrator;
using MessageStorage.DataAccessLayer;

namespace MessageStorage.Postgres.Migrations
{
    [Migration(7)]
    public class _0007_CreateIndex__JobTable_MessageId : Migration
    {
        private readonly RepositoryConfiguration _repositoryConfiguration;

        public _0007_CreateIndex__JobTable_MessageId(RepositoryConfiguration repositoryConfiguration)
        {
            _repositoryConfiguration = repositoryConfiguration;
        }

        public override void Up()
        {
            Create.Index("IX_Jobs_MessageId")
                  .OnTable("jobs")
                  .InSchema(_repositoryConfiguration.Schema)
                  .OnColumn("message_id");
        }

        public override void Down()
        {
            Delete.Index("IX_Jobs_MessageId")
                  .OnTable("jobs")
                  .InSchema(_repositoryConfiguration.Schema);
        }
    }
}