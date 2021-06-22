using FluentMigrator;
using MessageStorage.DataAccessLayer;

namespace MessageStorage.SqlServer.Migrations
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
            Create.Table("JobStatuses")
                  .InSchema(_repositoryConfiguration.Schema)
                  .WithColumn("Id").AsInt32().PrimaryKey("PK_JobStatuses_Id")
                  .WithColumn("StatusName").AsString().NotNullable();
        }

        public override void Down()
        {
            Delete.Table("JobStatuses")
                  .InSchema(_repositoryConfiguration.Schema);
        }
    }
}