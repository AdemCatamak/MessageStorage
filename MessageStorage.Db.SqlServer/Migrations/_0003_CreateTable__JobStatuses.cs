using FluentMigrator;
using MessageStorage.Db.Configurations;

namespace MessageStorage.Db.SqlServer.Migrations
{
    [Migration(version: 3)]
    public class _0003_CreateTable__JobStatuses : Migration
    {
        private readonly DbRepositoryConfiguration _dbRepositoryConfiguration;

        public _0003_CreateTable__JobStatuses(DbRepositoryConfiguration dbRepositoryConfiguration)
        {
            _dbRepositoryConfiguration = dbRepositoryConfiguration;
        }

        public override void Up()
        {
            Create.Table("JobStatuses")
                  .InSchema(_dbRepositoryConfiguration.Schema)
                  .WithColumn("StatusId").AsInt32().PrimaryKey("PK_JobStatuses_StatusId")
                  .WithColumn("StatusName").AsString().NotNullable();
        }

        public override void Down()
        {
            Delete.Table("JobStatuses")
                  .InSchema(_dbRepositoryConfiguration.Schema);
        }
    }
}