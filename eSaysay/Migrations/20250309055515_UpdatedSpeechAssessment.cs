using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eSaysay.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedSpeechAssessment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SpeechAssessment_InteractiveExercises_ExerciseID",
                table: "SpeechAssessment");

            migrationBuilder.DropIndex(
                name: "IX_SpeechAssessment_ExerciseID",
                table: "SpeechAssessment");

            migrationBuilder.RenameColumn(
                name: "ExerciseID",
                table: "SpeechAssessment",
                newName: "WordCount");

            migrationBuilder.AddColumn<double>(
                name: "FluencyScore",
                table: "SpeechAssessment",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "OverallScore",
                table: "SpeechAssessment",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SpeechDuration",
                table: "SpeechAssessment",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FluencyScore",
                table: "SpeechAssessment");

            migrationBuilder.DropColumn(
                name: "OverallScore",
                table: "SpeechAssessment");

            migrationBuilder.DropColumn(
                name: "SpeechDuration",
                table: "SpeechAssessment");

            migrationBuilder.RenameColumn(
                name: "WordCount",
                table: "SpeechAssessment",
                newName: "ExerciseID");

            migrationBuilder.CreateIndex(
                name: "IX_SpeechAssessment_ExerciseID",
                table: "SpeechAssessment",
                column: "ExerciseID");

            migrationBuilder.AddForeignKey(
                name: "FK_SpeechAssessment_InteractiveExercises_ExerciseID",
                table: "SpeechAssessment",
                column: "ExerciseID",
                principalTable: "InteractiveExercises",
                principalColumn: "ExerciseID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
