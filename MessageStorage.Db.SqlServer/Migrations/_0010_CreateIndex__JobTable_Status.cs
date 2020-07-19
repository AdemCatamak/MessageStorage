using FluentMigrator;
using MessageStorage.Db.Configurations;

namespace MessageStorage.Db.SqlServer.Migrations
{
    [Migration(10)]
    public class _0010_CreateIndex__JobTable_Status : Migration
    {
        private readonly DbRepositoryConfiguration _dbRepositoryConfiguration;

        public _0010_CreateIndex__JobTable_Status(DbRepositoryConfiguration dbRepositoryConfiguration)
        {
            _dbRepositoryConfiguration = dbRepositoryConfiguration;
        }

        public override void Up()
        {
            Create.Index("Jobs_JobStatus")
                  .OnTable("Jobs")
                  .InSchema(_dbRepositoryConfiguration.Schema)
                  .OnColumn("JobStatus");
        }

        public override void Down()
        {
            Delete.Index("Jobs_JobStatus")
                  .OnTable("Jobs")
                  .InSchema(_dbRepositoryConfiguration.Schema);
        }
    }
}