using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoStudy.Migrations
{
    /// <inheritdoc />
    public partial class AddImportantQuestionTypesAndFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "QuestionText",
                table: "ImportantQuestions",
                newName: "Question");

            migrationBuilder.AlterColumn<string>(
                name: "ExamReference",
                table: "ImportantQuestions",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DifficultyLevel",
                table: "ImportantQuestions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "QuestionType",
                table: "ImportantQuestions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Short");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ImportantQuestions",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuestionType",
                table: "ImportantQuestions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ImportantQuestions");

            migrationBuilder.RenameColumn(
                name: "Question",
                table: "ImportantQuestions",
                newName: "QuestionText");

            migrationBuilder.AlterColumn<string>(
                name: "ExamReference",
                table: "ImportantQuestions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DifficultyLevel",
                table: "ImportantQuestions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }
    }
}
