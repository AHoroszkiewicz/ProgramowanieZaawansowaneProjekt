using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartyTavern.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGamesProposalsUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameProposals_AspNetUsers_SubmittedByUserId",
                table: "GameProposals");

            migrationBuilder.AlterColumn<string>(
                name: "SubmittedByUserId",
                table: "GameProposals",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_GameProposals_AspNetUsers_SubmittedByUserId",
                table: "GameProposals",
                column: "SubmittedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameProposals_AspNetUsers_SubmittedByUserId",
                table: "GameProposals");

            migrationBuilder.AlterColumn<string>(
                name: "SubmittedByUserId",
                table: "GameProposals",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GameProposals_AspNetUsers_SubmittedByUserId",
                table: "GameProposals",
                column: "SubmittedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
