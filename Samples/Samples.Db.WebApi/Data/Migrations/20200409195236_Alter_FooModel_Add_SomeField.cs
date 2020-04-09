using Microsoft.EntityFrameworkCore.Migrations;

namespace Samples.Db.WebApi.Data.Migrations
{
    public partial class Alter_FooModel_Add_SomeField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SomeField",
                table: "Foo",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SomeField",
                table: "Foo");
        }
    }
}
