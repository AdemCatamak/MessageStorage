using System;
using FluentMigrator;

namespace MessageStorage.Postgres.DataAccessLayer.Migrations;

[Migration(version: 4)]
public class M0004CreateTableMessageTable : Migration
{
    private readonly PostgresRepositoryContextConfiguration _repositoryContextConfiguration;

    public M0004CreateTableMessageTable(PostgresRepositoryContextConfiguration repositoryContextConfiguration)
    {
        _repositoryContextConfiguration = repositoryContextConfiguration;
    }

    public override void Up()
    {
        Create.Table("messages")
              .InSchema(_repositoryContextConfiguration.Schema)
              .WithColumn("id").AsGuid().PrimaryKey("PK_Messages_Id")
              .WithColumn("created_on").AsDateTime2().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
              .WithColumn("payload_type_name").AsString(Int32.MaxValue).Nullable()
              .WithColumn("payload").AsString(int.MaxValue).NotNullable()
            ;
    }

    public override void Down()
    {
        Delete.Table("messages")
              .InSchema(_repositoryContextConfiguration.Schema);
    }
}