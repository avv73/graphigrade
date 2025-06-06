﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraphiGrade.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFacultyNumberIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_FacultyNumber",
                schema: "identity",
                table: "AspNetUsers",
                column: "FacultyNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_FacultyNumber",
                schema: "identity",
                table: "AspNetUsers");
        }
    }
}
