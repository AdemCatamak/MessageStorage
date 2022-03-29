using System;
using FluentMigrator;

namespace MessageStorage.SqlServer.DataAccessLayer.Migrations;

[Migration(version: 4)]
public class M0004CreateTableMessageTable : Migration
{
    private readonly SqlServerRepositoryContextConfiguration _repositoryContextConfiguration;

    public M0004CreateTableMessageTable(SqlServerRepositoryContextConfiguration repositoryContextConfiguration)
    {
        _repositoryContextConfiguration = repositoryContextConfiguration;
    }

    public override void Up()
    {
        Create.Table("Messages")
              .InSchema(_repositoryContextConfiguration.Schema)
              .WithColumn("Id").AsGuid().PrimaryKey("PK_Messages_Id")
              .WithColumn("CreatedOn").AsDateTime2().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
              .WithColumn("PayloadTypeName").AsString(Int32.MaxValue).Nullable()
              .WithColumn("Payload").AsString(int.MaxValue).NotNullable()
            ;
    }

    public override void Down()
    {
        Delete.Table("Messages")
              .InSchema(_repositoryContextConfiguration.Schema);
    }
}