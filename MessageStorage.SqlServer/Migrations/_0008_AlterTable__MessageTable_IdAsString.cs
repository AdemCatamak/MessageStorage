using FluentMigrator;
using MessageStorage.Configurations;

namespace MessageStorage.SqlServer.Migrations
{
    [Migration(version: 8)]
    public class _0008_AlterTable__MessageTable_IdAsString : Migration
    {
        private readonly MessageStorageRepositoryContextConfiguration _messageStorageRepositoryContextConfiguration;

        public _0008_AlterTable__MessageTable_IdAsString(MessageStorageRepositoryContextConfiguration messageStorageRepositoryContextConfiguration)
        {
            _messageStorageRepositoryContextConfiguration = messageStorageRepositoryContextConfiguration;
        }

        public override void Up()
        {
            Execute.Sql(
                        CustomScriptBuilder.DeletePrimaryKeyScript(_messageStorageRepositoryContextConfiguration.Schema, "Messages")
                       );
            
            Alter.Table("Messages")
                 .InSchema(_messageStorageRepositoryContextConfiguration.Schema)
                 .AlterColumn("MessageId").AsString(size: 512);

            Create.PrimaryKey("PK_Messages_MessageId")
                  .OnTable("Messages")
                  .WithSchema(_messageStorageRepositoryContextConfiguration.Schema)
                  .Column("MessageId");
        }

        public override void Down()
        {
            Delete.PrimaryKey("PK_Messages_MessageId")
                  .FromTable("Messages")
                  .InSchema(_messageStorageRepositoryContextConfiguration.Schema);

            Alter.Table("Messages")
                 .InSchema(_messageStorageRepositoryContextConfiguration.Schema)
                 .AlterColumn("MessageId").AsInt64();

            Create.PrimaryKey("PK_Messages_MessageId")
                  .OnTable("Messages")
                  .WithSchema(_messageStorageRepositoryContextConfiguration.Schema)
                  .Column("MessageId");
        }
    }
}