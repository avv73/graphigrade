using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraphiGrade.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFacultyNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FacultyNumber",
                schema: "identity",
                table: "AspNetUsers",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FacultyNumber",
                schema: "identity",
                table: "AspNetUsers");
        }
    }
}
