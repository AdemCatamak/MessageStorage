using System;
using FluentMigrator;
using MessageStorage.DataAccessLayer;

namespace MessageStorage.MySql.Migrations
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
            Create.Table("messages")
                  .InSchema(_repositoryConfiguration.Schema)
                  .WithColumn("id").AsGuid().PrimaryKey("PK_Messages_Id")
                  .WithColumn("created_on").AsDateTime2().NotNullable()//.WithDefault(SystemMethods.CurrentUTCDateTime)
                  .WithColumn("payload_type_name").AsString(Int32.MaxValue).Nullable()
                  .WithColumn("payload").AsString(int.MaxValue).NotNullable()
                   ;
        }

        public override void Down()
        {
            Delete.Table("messages")
                  .InSchema(_repositoryConfiguration.Schema);
        }
    }
}