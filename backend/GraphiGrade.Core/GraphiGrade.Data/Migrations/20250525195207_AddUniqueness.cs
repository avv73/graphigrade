using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraphiGrade.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_FilesMetadata_ExpectedImageId",
                table: "Exercises");

            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_Users_CreatedById",
                table: "Exercises");

            migrationBuilder.DropForeignKey(
                name: "FK_ExercisesGroups_Exercises_ExerciseId",
                table: "ExercisesGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_ExercisesGroups_Groups_GroupId",
                table: "ExercisesGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_Exercises_ExerciseId",
                table: "Submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_FilesMetadata_ResultImageId",
                table: "Submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_FilesMetadata_SourceCodeId",
                table: "Submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_Users_UserId",
                table: "Submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_UsersGroups_Groups_GroupId",
                table: "UsersGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_UsersGroups_Users_UserId",
                table: "UsersGroups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Submissions",
                table: "Submissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Groups",
                table: "Groups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FilesMetadata",
                table: "FilesMetadata");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Exercises",
                table: "Exercises");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "User");

            migrationBuilder.RenameTable(
                name: "Submissions",
                newName: "Submission");

            migrationBuilder.RenameTable(
                name: "Groups",
                newName: "Group");

            migrationBuilder.RenameTable(
                name: "FilesMetadata",
                newName: "FileMetadata");

            migrationBuilder.RenameTable(
                name: "Exercises",
                newName: "Exercise");

            migrationBuilder.RenameIndex(
                name: "IX_Submissions_UserId",
                table: "Submission",
                newName: "IX_Submission_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Submissions_SourceCodeId",
                table: "Submission",
                newName: "IX_Submission_SourceCodeId");

            migrationBuilder.RenameIndex(
                name: "IX_Submissions_ResultImageId",
                table: "Submission",
                newName: "IX_Submission_ResultImageId");

            migrationBuilder.RenameIndex(
                name: "IX_Submissions_ExerciseId",
                table: "Submission",
                newName: "IX_Submission_ExerciseId");

            migrationBuilder.RenameIndex(
                name: "IX_Exercises_ExpectedImageId",
                table: "Exercise",
                newName: "IX_Exercise_ExpectedImageId");

            migrationBuilder.RenameIndex(
                name: "IX_Exercises_CreatedById",
                table: "Exercise",
                newName: "IX_Exercise_CreatedById");

            migrationBuilder.AddPrimaryKey(
                name: "PK_User",
                table: "User",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Submission",
                table: "Submission",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Group",
                table: "Group",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FileMetadata",
                table: "FileMetadata",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Exercise",
                table: "Exercise",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_User_Username",
                table: "User",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Submission_JudgeId",
                table: "Submission",
                column: "JudgeId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Exercise_FileMetadata_ExpectedImageId",
                table: "Exercise",
                column: "ExpectedImageId",
                principalTable: "FileMetadata",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Exercise_User_CreatedById",
                table: "Exercise",
                column: "CreatedById",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExercisesGroups_Exercise_ExerciseId",
                table: "ExercisesGroups",
                column: "ExerciseId",
                principalTable: "Exercise",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExercisesGroups_Group_GroupId",
                table: "ExercisesGroups",
                column: "GroupId",
                principalTable: "Group",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Submission_Exercise_ExerciseId",
                table: "Submission",
                column: "ExerciseId",
                principalTable: "Exercise",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Submission_FileMetadata_ResultImageId",
                table: "Submission",
                column: "ResultImageId",
                principalTable: "FileMetadata",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Submission_FileMetadata_SourceCodeId",
                table: "Submission",
                column: "SourceCodeId",
                principalTable: "FileMetadata",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Submission_User_UserId",
                table: "Submission",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UsersGroups_Group_GroupId",
                table: "UsersGroups",
                column: "GroupId",
                principalTable: "Group",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UsersGroups_User_UserId",
                table: "UsersGroups",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exercise_FileMetadata_ExpectedImageId",
                table: "Exercise");

            migrationBuilder.DropForeignKey(
                name: "FK_Exercise_User_CreatedById",
                table: "Exercise");

            migrationBuilder.DropForeignKey(
                name: "FK_ExercisesGroups_Exercise_ExerciseId",
                table: "ExercisesGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_ExercisesGroups_Group_GroupId",
                table: "ExercisesGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_Submission_Exercise_ExerciseId",
                table: "Submission");

            migrationBuilder.DropForeignKey(
                name: "FK_Submission_FileMetadata_ResultImageId",
                table: "Submission");

            migrationBuilder.DropForeignKey(
                name: "FK_Submission_FileMetadata_SourceCodeId",
                table: "Submission");

            migrationBuilder.DropForeignKey(
                name: "FK_Submission_User_UserId",
                table: "Submission");

            migrationBuilder.DropForeignKey(
                name: "FK_UsersGroups_Group_GroupId",
                table: "UsersGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_UsersGroups_User_UserId",
                table: "UsersGroups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_User",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_Username",
                table: "User");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Submission",
                table: "Submission");

            migrationBuilder.DropIndex(
                name: "IX_Submission_JudgeId",
                table: "Submission");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Group",
                table: "Group");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FileMetadata",
                table: "FileMetadata");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Exercise",
                table: "Exercise");

            migrationBuilder.RenameTable(
                name: "User",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "Submission",
                newName: "Submissions");

            migrationBuilder.RenameTable(
                name: "Group",
                newName: "Groups");

            migrationBuilder.RenameTable(
                name: "FileMetadata",
                newName: "FilesMetadata");

            migrationBuilder.RenameTable(
                name: "Exercise",
                newName: "Exercises");

            migrationBuilder.RenameIndex(
                name: "IX_Submission_UserId",
                table: "Submissions",
                newName: "IX_Submissions_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Submission_SourceCodeId",
                table: "Submissions",
                newName: "IX_Submissions_SourceCodeId");

            migrationBuilder.RenameIndex(
                name: "IX_Submission_ResultImageId",
                table: "Submissions",
                newName: "IX_Submissions_ResultImageId");

            migrationBuilder.RenameIndex(
                name: "IX_Submission_ExerciseId",
                table: "Submissions",
                newName: "IX_Submissions_ExerciseId");

            migrationBuilder.RenameIndex(
                name: "IX_Exercise_ExpectedImageId",
                table: "Exercises",
                newName: "IX_Exercises_ExpectedImageId");

            migrationBuilder.RenameIndex(
                name: "IX_Exercise_CreatedById",
                table: "Exercises",
                newName: "IX_Exercises_CreatedById");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Submissions",
                table: "Submissions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Groups",
                table: "Groups",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FilesMetadata",
                table: "FilesMetadata",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Exercises",
                table: "Exercises",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_FilesMetadata_ExpectedImageId",
                table: "Exercises",
                column: "ExpectedImageId",
                principalTable: "FilesMetadata",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_Users_CreatedById",
                table: "Exercises",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExercisesGroups_Exercises_ExerciseId",
                table: "ExercisesGroups",
                column: "ExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExercisesGroups_Groups_GroupId",
                table: "ExercisesGroups",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_Exercises_ExerciseId",
                table: "Submissions",
                column: "ExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_FilesMetadata_ResultImageId",
                table: "Submissions",
                column: "ResultImageId",
                principalTable: "FilesMetadata",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_FilesMetadata_SourceCodeId",
                table: "Submissions",
                column: "SourceCodeId",
                principalTable: "FilesMetadata",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_Users_UserId",
                table: "Submissions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UsersGroups_Groups_GroupId",
                table: "UsersGroups",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UsersGroups_Users_UserId",
                table: "UsersGroups",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
