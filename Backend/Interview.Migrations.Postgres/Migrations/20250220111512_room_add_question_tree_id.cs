using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Interview.Migrations.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class room_add_question_tree_id : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "QuestionTreeId",
                table: "Rooms",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_QuestionTreeId",
                table: "Rooms",
                column: "QuestionTreeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_QuestionTree_QuestionTreeId",
                table: "Rooms",
                column: "QuestionTreeId",
                principalTable: "QuestionTree",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_QuestionTree_QuestionTreeId",
                table: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_QuestionTreeId",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "QuestionTreeId",
                table: "Rooms");
        }
    }
}
