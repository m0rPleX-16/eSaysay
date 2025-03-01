using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eSaysay.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedExercise : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentTranslate",
                table: "InteractiveExercises",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "InteractiveExercises",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentTranslate",
                table: "InteractiveExercises");

            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "InteractiveExercises");
        }
    }
}
