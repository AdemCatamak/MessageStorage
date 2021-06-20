using FluentMigrator;
using MessageStorage.DataAccessLayer;

namespace MessageStorage.Postgres.Migrations
{
    [Migration(version: 2)]
    public class _0002_CreateTable__JobStatuses : Migration
    {
        private readonly RepositoryConfiguration _repositoryConfiguration;

        public _0002_CreateTable__JobStatuses(RepositoryConfiguration repositoryConfiguration)
        {
            _repositoryConfiguration = repositoryConfiguration;
        }

        public override void Up()
        {
            Create.Table("job_statuses")
                  .InSchema(_repositoryConfiguration.Schema)
                  .WithColumn("id").AsInt32().PrimaryKey("PK_JobStatuses_Id")
                  .WithColumn("status_name").AsString().NotNullable();
        }

        public override void Down()
        {
            Delete.Table("job_statuses")
                  .InSchema(_repositoryConfiguration.Schema);
        }
    }
}