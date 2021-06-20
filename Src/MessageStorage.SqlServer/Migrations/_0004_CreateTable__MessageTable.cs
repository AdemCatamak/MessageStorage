using System;
using FluentMigrator;
using MessageStorage.DataAccessLayer;

namespace MessageStorage.SqlServer.Migrations
{
    [Migration(version: 4)]
    public class _0004_CreateTable__MessageTable : Migration
    {
        private readonly RepositoryConfiguration _repositoryConfiguration;

        public _0004_CreateTable__MessageTable(RepositoryConfiguration repositoryConfiguration)
        {
            _repositoryConfiguration = repositoryConfiguration;
        }

        public override void Up()
        {
            Create.Table("Messages")
                  .InSchema(_repositoryConfiguration.Schema)
                  .WithColumn("Id").AsGuid().PrimaryKey("PK_Messages_Id")
                  .WithColumn("CreatedOn").AsDateTime2().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
                  .WithColumn("PayloadTypeName").AsString(Int32.MaxValue).Nullable()
                  .WithColumn("Payload").AsString(int.MaxValue).NotNullable()
                   ;
        }

        public override void Down()
        {
            Delete.Table("Messages")
                  .InSchema(_repositoryConfiguration.Schema);
        }
    }
}