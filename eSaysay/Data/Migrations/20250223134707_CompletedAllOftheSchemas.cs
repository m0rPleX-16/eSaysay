using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eSaysay.Data.Migrations
{
    /// <inheritdoc />
    public partial class CompletedAllOftheSchemas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lesson_AspNetUsers_CreatedBy",
                table: "Lesson");

            migrationBuilder.DropIndex(
                name: "IX_Lesson_CreatedBy",
                table: "Lesson");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Lesson",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Lesson",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AdaptiveLearning",
                columns: table => new
                {
                    AdaptiveID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CurrentLevel = table.Column<int>(type: "int", nullable: false),
                    RecommendedLessons = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdaptiveLearning", x => x.AdaptiveID);
                    table.ForeignKey(
                        name: "FK_AdaptiveLearning_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Analytics",
                columns: table => new
                {
                    AnalyticsID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LessonCompleted = table.Column<int>(type: "int", nullable: false),
                    AverageScore = table.Column<double>(type: "float", nullable: false),
                    TimeSpent = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Analytics", x => x.AnalyticsID);
                    table.ForeignKey(
                        name: "FK_Analytics_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Analytics_Lesson_LessonCompleted",
                        column: x => x.LessonCompleted,
                        principalTable: "Lesson",
                        principalColumn: "LessonID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Badges",
                columns: table => new
                {
                    BadgeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BadgeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Criteria = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Badges", x => x.BadgeID);
                });

            migrationBuilder.CreateTable(
                name: "SpeechAssessment",
                columns: table => new
                {
                    AssessmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ExerciseID = table.Column<int>(type: "int", nullable: false),
                    UserRecording = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    AccuracyScore = table.Column<double>(type: "float", nullable: false),
                    Feedback = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AttemptDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpeechAssessment", x => x.AssessmentID);
                    table.ForeignKey(
                        name: "FK_SpeechAssessment_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpeechAssessment_InteractiveExercises_ExerciseID",
                        column: x => x.ExerciseID,
                        principalTable: "InteractiveExercises",
                        principalColumn: "ExerciseID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserProgress",
                columns: table => new
                {
                    ProgressID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LessonID = table.Column<int>(type: "int", nullable: false),
                    CompletionStatus = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    Score = table.Column<double>(type: "float", nullable: true),
                    TimeSpent = table.Column<int>(type: "int", nullable: false),
                    LastAccessedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProgress", x => x.ProgressID);
                    table.ForeignKey(
                        name: "FK_UserProgress_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserProgress_Lesson_LessonID",
                        column: x => x.LessonID,
                        principalTable: "Lesson",
                        principalColumn: "LessonID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserResponse",
                columns: table => new
                {
                    ResponseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ExerciseID = table.Column<int>(type: "int", nullable: false),
                    UserAnswer = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    AttemptDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserResponse", x => x.ResponseID);
                    table.ForeignKey(
                        name: "FK_UserResponse_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserResponse_InteractiveExercises_ExerciseID",
                        column: x => x.ExerciseID,
                        principalTable: "InteractiveExercises",
                        principalColumn: "ExerciseID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserBadges",
                columns: table => new
                {
                    UserBadgeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BadgeID = table.Column<int>(type: "int", nullable: false),
                    DateEarned = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBadges", x => x.UserBadgeID);
                    table.ForeignKey(
                        name: "FK_UserBadges_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserBadges_Badges_BadgeID",
                        column: x => x.BadgeID,
                        principalTable: "Badges",
                        principalColumn: "BadgeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lesson_CreatedByUserId",
                table: "Lesson",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AdaptiveLearning_UserID",
                table: "AdaptiveLearning",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Analytics_LessonCompleted",
                table: "Analytics",
                column: "LessonCompleted");

            migrationBuilder.CreateIndex(
                name: "IX_Analytics_UserID",
                table: "Analytics",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_SpeechAssessment_ExerciseID",
                table: "SpeechAssessment",
                column: "ExerciseID");

            migrationBuilder.CreateIndex(
                name: "IX_SpeechAssessment_UserID",
                table: "SpeechAssessment",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserBadges_BadgeID",
                table: "UserBadges",
                column: "BadgeID");

            migrationBuilder.CreateIndex(
                name: "IX_UserBadges_UserID",
                table: "UserBadges",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserProgress_LessonID",
                table: "UserProgress",
                column: "LessonID");

            migrationBuilder.CreateIndex(
                name: "IX_UserProgress_UserID",
                table: "UserProgress",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserResponse_ExerciseID",
                table: "UserResponse",
                column: "ExerciseID");

            migrationBuilder.CreateIndex(
                name: "IX_UserResponse_UserID",
                table: "UserResponse",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Lesson_AspNetUsers_CreatedByUserId",
                table: "Lesson",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lesson_AspNetUsers_CreatedByUserId",
                table: "Lesson");

            migrationBuilder.DropTable(
                name: "AdaptiveLearning");

            migrationBuilder.DropTable(
                name: "Analytics");

            migrationBuilder.DropTable(
                name: "SpeechAssessment");

            migrationBuilder.DropTable(
                name: "UserBadges");

            migrationBuilder.DropTable(
                name: "UserProgress");

            migrationBuilder.DropTable(
                name: "UserResponse");

            migrationBuilder.DropTable(
                name: "Badges");

            migrationBuilder.DropIndex(
                name: "IX_Lesson_CreatedByUserId",
                table: "Lesson");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Lesson");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Lesson",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Lesson_CreatedBy",
                table: "Lesson",
                column: "CreatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Lesson_AspNetUsers_CreatedBy",
                table: "Lesson",
                column: "CreatedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
