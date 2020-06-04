using Microsoft.EntityFrameworkCore.Migrations;

namespace access.analyser.Migrations
{
    public partial class S3ObjectKeyLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "S3ObjectKey",
                table: "Logs",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "S3ObjectKey",
                table: "Logs");
        }
    }
}
