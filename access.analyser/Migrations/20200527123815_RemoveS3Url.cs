using Microsoft.EntityFrameworkCore.Migrations;

namespace access.analyser.Migrations
{
    public partial class RemoveS3Url : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogS3Url",
                table: "Logs");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogS3Url",
                table: "Logs",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
