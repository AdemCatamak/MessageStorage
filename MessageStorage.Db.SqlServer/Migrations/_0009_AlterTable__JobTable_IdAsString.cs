using FluentMigrator;
using MessageStorage.Db.Configurations;

namespace MessageStorage.Db.SqlServer.Migrations
{
    [Migration(version: 9)]
    public class _0009_AlterTable__JobTable_IdAsString : Migration
    {
        private readonly DbRepositoryConfiguration _dbRepositoryConfiguration;

        public _0009_AlterTable__JobTable_IdAsString(DbRepositoryConfiguration dbRepositoryConfiguration)
        {
            _dbRepositoryConfiguration = dbRepositoryConfiguration;
        }

        public override void Up()
        {
            Execute.Sql(CustomScriptBuilder.DeletePrimaryKeyScript(_dbRepositoryConfiguration.Schema, "Jobs"));

            Alter.Table("Jobs")
                 .InSchema(_dbRepositoryConfiguration.Schema)
                 .AlterColumn("JobId").AsString(size: 512);

            Create.PrimaryKey("PK_Jobs_JobId")
                  .OnTable("Jobs")
                  .WithSchema(_dbRepositoryConfiguration.Schema)
                  .Column("JobId");
        }

        public override void Down()
        {
            Delete.PrimaryKey("PK_Jobs_JobId")
                  .FromTable("Jobs")
                  .InSchema(_dbRepositoryConfiguration.Schema);

            Alter.Table("Jobs")
                 .InSchema(_dbRepositoryConfiguration.Schema)
                 .AlterColumn("JobId").AsInt64();

            Create.PrimaryKey("PK_Jobs_JobId")
                  .OnTable("Jobs")
                  .WithSchema(_dbRepositoryConfiguration.Schema)
                  .Column("JobId");
        }
    }
}