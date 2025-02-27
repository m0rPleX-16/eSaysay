using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eSaysay.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedLesson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Analytics_Lesson_LessonCompleted",
                table: "Analytics");

            migrationBuilder.DropForeignKey(
                name: "FK_InteractiveExercises_Lesson_LessonID",
                table: "InteractiveExercises");

            migrationBuilder.DropForeignKey(
                name: "FK_Lesson_AspNetUsers_CreatedByUserId",
                table: "Lesson");

            migrationBuilder.DropForeignKey(
                name: "FK_Lesson_Language_LanguageID",
                table: "Lesson");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProgress_Lesson_LessonID",
                table: "UserProgress");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Lesson",
                table: "Lesson");

            migrationBuilder.RenameTable(
                name: "Lesson",
                newName: "Lessons");

            migrationBuilder.RenameIndex(
                name: "IX_Lesson_LanguageID",
                table: "Lessons",
                newName: "IX_Lessons_LanguageID");

            migrationBuilder.RenameIndex(
                name: "IX_Lesson_CreatedByUserId",
                table: "Lessons",
                newName: "IX_Lessons_CreatedByUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Lessons",
                table: "Lessons",
                column: "LessonID");

            migrationBuilder.AddForeignKey(
                name: "FK_Analytics_Lessons_LessonCompleted",
                table: "Analytics",
                column: "LessonCompleted",
                principalTable: "Lessons",
                principalColumn: "LessonID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InteractiveExercises_Lessons_LessonID",
                table: "InteractiveExercises",
                column: "LessonID",
                principalTable: "Lessons",
                principalColumn: "LessonID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Lessons_AspNetUsers_CreatedByUserId",
                table: "Lessons",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Lessons_Language_LanguageID",
                table: "Lessons",
                column: "LanguageID",
                principalTable: "Language",
                principalColumn: "LanguageID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserProgress_Lessons_LessonID",
                table: "UserProgress",
                column: "LessonID",
                principalTable: "Lessons",
                principalColumn: "LessonID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Analytics_Lessons_LessonCompleted",
                table: "Analytics");

            migrationBuilder.DropForeignKey(
                name: "FK_InteractiveExercises_Lessons_LessonID",
                table: "InteractiveExercises");

            migrationBuilder.DropForeignKey(
                name: "FK_Lessons_AspNetUsers_CreatedByUserId",
                table: "Lessons");

            migrationBuilder.DropForeignKey(
                name: "FK_Lessons_Language_LanguageID",
                table: "Lessons");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProgress_Lessons_LessonID",
                table: "UserProgress");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Lessons",
                table: "Lessons");

            migrationBuilder.RenameTable(
                name: "Lessons",
                newName: "Lesson");

            migrationBuilder.RenameIndex(
                name: "IX_Lessons_LanguageID",
                table: "Lesson",
                newName: "IX_Lesson_LanguageID");

            migrationBuilder.RenameIndex(
                name: "IX_Lessons_CreatedByUserId",
                table: "Lesson",
                newName: "IX_Lesson_CreatedByUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Lesson",
                table: "Lesson",
                column: "LessonID");

            migrationBuilder.AddForeignKey(
                name: "FK_Analytics_Lesson_LessonCompleted",
                table: "Analytics",
                column: "LessonCompleted",
                principalTable: "Lesson",
                principalColumn: "LessonID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InteractiveExercises_Lesson_LessonID",
                table: "InteractiveExercises",
                column: "LessonID",
                principalTable: "Lesson",
                principalColumn: "LessonID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Lesson_AspNetUsers_CreatedByUserId",
                table: "Lesson",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Lesson_Language_LanguageID",
                table: "Lesson",
                column: "LanguageID",
                principalTable: "Language",
                principalColumn: "LanguageID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserProgress_Lesson_LessonID",
                table: "UserProgress",
                column: "LessonID",
                principalTable: "Lesson",
                principalColumn: "LessonID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
