using System;
using FluentMigrator;
using MessageStorage.Configurations;

namespace MessageStorage.SqlServer.Migrations
{
    [Migration(version: 5)]
    public class _0005_CreateTable__MessageTable : Migration
    {
        private readonly MessageStorageRepositoryContextConfiguration _messageStorageRepositoryContextConfiguration;

        public _0005_CreateTable__MessageTable(MessageStorageRepositoryContextConfiguration messageStorageRepositoryContextConfiguration)
        {
            _messageStorageRepositoryContextConfiguration = messageStorageRepositoryContextConfiguration;
        }

        public override void Up()
        {
            Create.Table("Messages")
                  .InSchema(_messageStorageRepositoryContextConfiguration.Schema)
                  .WithColumn("MessageId").AsInt64().PrimaryKey("PK_Messages_MessageId")
                  .WithColumn("CreatedOn").AsDateTime2().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
                  .WithColumn("SerializedPayload").AsString(int.MaxValue).NotNullable()
                  .WithColumn("PayloadClassName").AsString(Int32.MaxValue).Nullable()
                  .WithColumn("PayloadClassFullName").AsString(Int32.MaxValue).Nullable();
        }

        public override void Down()
        {
            Delete.Table("Messages")
                  .InSchema(_messageStorageRepositoryContextConfiguration.Schema);
        }
    }
}