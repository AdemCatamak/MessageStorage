using System;
using FluentMigrator;
using MessageStorage.Db.Configurations;

namespace MessageStorage.Db.SqlServer.Migrations
{
    [Migration(version: 5)]
    public class _0005_CreateTable__MessageTable : Migration
    {
        private readonly DbRepositoryConfiguration _dbRepositoryConfiguration;

        public _0005_CreateTable__MessageTable(DbRepositoryConfiguration dbRepositoryConfiguration)
        {
            _dbRepositoryConfiguration = dbRepositoryConfiguration;
        }

        public override void Up()
        {
            Create.Table("Messages")
                  .InSchema(_dbRepositoryConfiguration.Schema)
                  .WithColumn("MessageId").AsInt64().PrimaryKey("PK_Messages_MessageId")
                  .WithColumn("CreatedOn").AsDateTime2().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
                  .WithColumn("SerializedPayload").AsString(int.MaxValue).NotNullable()
                  .WithColumn("PayloadClassName").AsString(Int32.MaxValue).Nullable()
                  .WithColumn("PayloadClassFullName").AsString(Int32.MaxValue).Nullable();
        }

        public override void Down()
        {
            Delete.Table("Messages")
                  .InSchema(_dbRepositoryConfiguration.Schema);
        }
    }
}