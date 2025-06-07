using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraphiGrade.Migrations
{
    /// <inheritdoc />
    public partial class ChangePasswordToChar60 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "User",
                type: "CHAR(60)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "CHAR(64)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "User",
                type: "CHAR(64)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "CHAR(60)");
        }
    }
}
