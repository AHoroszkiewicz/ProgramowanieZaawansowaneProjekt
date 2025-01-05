using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartyTavern.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSubmittedByUserFromGameProposal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameProposals_AspNetUsers_SubmittedByUserId",
                table: "GameProposals");

            migrationBuilder.DropIndex(
                name: "IX_GameProposals_SubmittedByUserId",
                table: "GameProposals");

            migrationBuilder.DropColumn(
                name: "SubmittedByUserId",
                table: "GameProposals");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SubmittedByUserId",
                table: "GameProposals",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameProposals_SubmittedByUserId",
                table: "GameProposals",
                column: "SubmittedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameProposals_AspNetUsers_SubmittedByUserId",
                table: "GameProposals",
                column: "SubmittedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
